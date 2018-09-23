using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ReactiveMaterial
{
    public class MaterialLight : MonoBehaviour, ILightDataListener
    {
        public enum LightsID { Static = 0, BackLights = 1, BigRingLights = 2, LeftLasers = 3, RightLasers = 4, TrackAndBottom = 5 }
        public enum ColorType { None = 0, LeftColor = 1, RightColor = 2}

        [Tooltip("Default Color")]
        public Color color = Color.white;

        [Tooltip("Sets the BaseColor to Custom Colors")]
        public ColorType CustomColor = ColorType.None;

        public LightsID lightsID = LightsID.Static;

        public List<Material> materials;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void Start()
        {
            try
            {
                meshFilter = this.gameObject.GetComponent<MeshFilter>();
                meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
                if (meshFilter == null)
                    meshFilter = this.gameObject.AddComponent<MeshFilter>();
                if (meshRenderer == null)
                    meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
                
                if (gameObject.GetComponentInChildren<MaterialLightManager>(true) == null)
                {
                    MaterialLightManager tlm = gameObject.AddComponent<MaterialLightManager>();
                    tlm.CreateMaterialLights(gameObject);
                }
                if (CustomColor == ColorType.None)
                    return;
                var colorManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();
                if (colorManager == null) return;

                var leftColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_colorA");
                var rightColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_colorB");
                color = (CustomColor == ColorType.LeftColor) ? leftColor.color : rightColor.color;
                foreach (Material mat in materials)
                {
                    if (mat.HasProperty("_BaseColor"))
                    {
                        color.a = mat.GetColor("_BaseColor").a;
                        mat.SetColor("_BaseColor", color);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public void OnColorChanged(Color color)
        {
            try
            {
                //Console.WriteLine("Color Set : " + color);
                foreach (Material mat in materials)
                {
                    mat.color = color;
                    mat.SetFloat("_Glow", color.a);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
            }
        }

    }
    
}
