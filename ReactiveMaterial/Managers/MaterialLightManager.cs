using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveMaterial
{
    public class MaterialLightManager : MonoBehaviour
    {
        private List<BloomPrePassLight> mbppLights;
        private List<MaterialLight> materialLightDescriptors;

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;

            SetColorToDefault();
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            SetColorToDefault();
        }

        private void SetColorToDefault()
        {
            try
            {
                materialLightDescriptors = GameObject.FindObjectsOfType<MaterialLight>().ToList();

                foreach (MaterialLight ml in materialLightDescriptors)
                {
                    ColorBloomPrePassLight colorBloom = ml.gameObject.GetComponent<ColorBloomPrePassLight>();
                    if (colorBloom != null)
                    {
                        colorBloom.color = ml.color;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void CreateTubeLights(GameObject go)
        {
            try
            {
                if (mbppLights == null) mbppLights = new List<BloomPrePassLight>();
                if (materialLightDescriptors == null) materialLightDescriptors = new List<MaterialLight>();

                MaterialLight[] localDescriptors = go.GetComponentsInChildren<MaterialLight>(true);

                if (localDescriptors == null) return;

                foreach (MaterialLight ml in localDescriptors)
                {
                    ColorBloomPrePassLight colorBloomLight;

                    colorBloomLight = ml.gameObject.AddComponent<ColorBloomPrePassLight>();
                    colorBloomLight.Init();
                    colorBloomLight.materialLight = ml;

                    colorBloomLight.color = ml.color;
                    ReflectionUtil.SetPrivateFieldBase(colorBloomLight, "_ID", (int)ml.lightsID);

                    mbppLights.Add(colorBloomLight);
                    materialLightDescriptors.Add(ml);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DestroyTubeLights()
        {
            try
            {
                for (int i = 0; i < mbppLights.Count; i++)
                {
                    GameObject.Destroy(mbppLights.Last());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
