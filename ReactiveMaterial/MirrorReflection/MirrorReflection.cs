// Modified from http://wiki.unity3d.com/index.php/MirrorReflection4 
// By PureDark

using System.Collections;
using UnityEngine;

namespace ReactiveMaterial
{
    [ExecuteInEditMode] // Make mirror live-update even when not in play mode
    public class MirrorReflection : MonoBehaviour
    {
        public bool m_DisablePixelLights = true;
        public int m_TextureSize = 1024;
        public float m_ClipPlaneOffset = 0.07f;

        public LayerMask m_ReflectLayers = -1;

        private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table

        private RenderTexture m_ReflectionTexture0 = null;
        private RenderTexture m_ReflectionTexture1 = null;
        private int m_OldReflectionTextureSize = 0;

        private static bool s_InsideRendering = false;


        // This is called when it's known that the object will be rendered by some
        // camera. We render reflections and do other updates here.
        // Because the script executes in edit mode, reflections for the scene view
        // camera will just work!
        public void OnWillRenderObject()
        {
            var rend = GetComponent<Renderer>();
            if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
                return;
            
            Camera cam = Camera.current;
            if (!cam)
            {
                return;
            }

            // Safeguard from recursive mirror reflections.
            if (s_InsideRendering)
            {
                return;
            }
            s_InsideRendering = true;

            Camera reflectionCamera;
            CreateMirrorObjects(cam, out reflectionCamera);

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Optionally disable pixel lights for reflection/refraction
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (m_DisablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            UpdateCameraModes(cam, reflectionCamera);

            // Render reflection
            // Reflect camera around reflection plane
            if (cam.stereoEnabled)
            {
                if (cam.stereoTargetEye == StereoTargetEyeMask.Left || cam.stereoTargetEye == StereoTargetEyeMask.Both)
                {
                    Vector3 eyePos = cam.transform.TransformPoint(new Vector3(-0.5f * cam.stereoSeparation, 0, 0));
                    Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

                    RenderReflection(reflectionCamera, m_ReflectionTexture0, eyePos, cam.transform.rotation, projectionMatrix);
                    Material[] materials = rend.sharedMaterials;
                    foreach (Material mat in materials)
                    {
                        if (mat.HasProperty("_ReflectionTex0"))
                            mat.SetTexture("_ReflectionTex0", m_ReflectionTexture0);
                    }
                }

                if (cam.stereoTargetEye == StereoTargetEyeMask.Right || cam.stereoTargetEye == StereoTargetEyeMask.Both)
                {
                    Vector3 eyePos = cam.transform.TransformPoint(new Vector3(0.5f * cam.stereoSeparation, 0, 0));
                    Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

                    RenderReflection(reflectionCamera, m_ReflectionTexture1, eyePos, cam.transform.rotation, projectionMatrix);
                    Material[] materials = rend.sharedMaterials;
                    foreach (Material mat in materials)
                    {
                        if (mat.HasProperty("_ReflectionTex1"))
                            mat.SetTexture("_ReflectionTex1", m_ReflectionTexture1);
                    }
                }
            }
            else
            {
                RenderReflection(reflectionCamera, m_ReflectionTexture0, cam.transform.position, cam.transform.rotation, cam.projectionMatrix);
                GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex0", m_ReflectionTexture0);
            }

            // Restore pixel light count
            if (m_DisablePixelLights)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }

            s_InsideRendering = false;
        }
        void RenderReflection(Camera reflectionCamera, RenderTexture targetTexture, Vector3 camPos, Quaternion camRot, Matrix4x4 camProjMatrix)
        {
            // Copy camera position/rotation/reflection into the reflectionCamera
            reflectionCamera.ResetWorldToCameraMatrix();
            reflectionCamera.transform.position = camPos;
            reflectionCamera.transform.rotation = camRot;
            reflectionCamera.projectionMatrix = camProjMatrix;
            reflectionCamera.targetTexture = targetTexture;
            reflectionCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            reflectionCamera.worldToCameraMatrix *= reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
            reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlane);

            // Set camera position and rotation
            reflectionCamera.transform.position = reflectionCamera.cameraToWorldMatrix.GetColumn(3);
            reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectionCamera.cameraToWorldMatrix.GetColumn(2), reflectionCamera.cameraToWorldMatrix.GetColumn(1));

            // never render water layer
            reflectionCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value;

            bool oldCulling = GL.invertCulling;
            GL.invertCulling = !oldCulling;
            reflectionCamera.Render();
            GL.invertCulling = oldCulling;
        }


        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (m_ReflectionTexture0)
            {
                DestroyImmediate(m_ReflectionTexture0);
                m_ReflectionTexture0 = null;
            }
            if (m_ReflectionTexture1)
            {
                DestroyImmediate(m_ReflectionTexture1);
                m_ReflectionTexture1 = null;
            }

            foreach (DictionaryEntry kvp in m_ReflectionCameras)
                DestroyImmediate(((Camera)kvp.Value).gameObject);
            m_ReflectionCameras.Clear();
        }


        void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            // set mirror camera to clear the same way as current camera
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent<Skybox>();
                Skybox mysky = dest.GetComponent<Skybox>();
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            if (!UnityEngine.XR.XRDevice.isPresent)
                dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }


        // On-demand create any objects we need for mirror
        void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
        {
            reflectionCamera = null;

            // Reflection render texture
            if (!m_ReflectionTexture0 || m_OldReflectionTextureSize != m_TextureSize)
            {
                if (m_ReflectionTexture0)
                    DestroyImmediate(m_ReflectionTexture0);
                if(m_TextureSize == 0)
                    m_ReflectionTexture0 = new RenderTexture(Screen.width, Screen.height, 24);
                else
                    m_ReflectionTexture0 = new RenderTexture(m_TextureSize, m_TextureSize, 24);
                m_ReflectionTexture0.name = "__MirrorReflection" + GetInstanceID();
                m_ReflectionTexture0.isPowerOfTwo = true;
                m_ReflectionTexture0.hideFlags = HideFlags.DontSave;
                m_OldReflectionTextureSize = m_TextureSize;
            }
            if (currentCamera.stereoEnabled && (!m_ReflectionTexture1 || m_OldReflectionTextureSize != m_TextureSize))
            {
                if (m_ReflectionTexture1)
                    DestroyImmediate(m_ReflectionTexture1);
                if (m_TextureSize == 0)
                    m_ReflectionTexture1 = new RenderTexture(Screen.width, Screen.height, 24);
                else
                    m_ReflectionTexture1 = new RenderTexture(m_TextureSize, m_TextureSize, 24);
                // m_ReflectionTexture1.name = "__MirrorReflection1" + GetInstanceID();
                m_ReflectionTexture1.isPowerOfTwo = true;
                m_ReflectionTexture1.hideFlags = HideFlags.DontSave;
                m_OldReflectionTextureSize = m_TextureSize;
            }

            // Camera for reflection
            reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
            if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
            {
                GameObject camClone = Camera.main?.gameObject ?? GameObject.FindGameObjectsWithTag("MainCamera")[0];
                GameObject go = Instantiate(camClone, Vector3.zero, Quaternion.identity, transform);
                go.name = "Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID();
                //GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
                reflectionCamera = go.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                reflectionCamera.transform.position = transform.position;
                reflectionCamera.transform.rotation = transform.rotation;
                reflectionCamera.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
                m_ReflectionCameras[currentCamera] = reflectionCamera;
            }
        }

        // Given position/normal of the plane, calculates plane in camera space.
        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
		    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);
 
		    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
 
		    reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		    reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		    reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		    reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);
 
		    reflectionMat.m30 = 0F;
		    reflectionMat.m31 = 0F;
		    reflectionMat.m32 = 0F;
		    reflectionMat.m33 = 1F;
        }
    }
}
