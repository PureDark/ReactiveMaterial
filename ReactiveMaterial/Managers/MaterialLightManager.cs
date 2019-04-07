using System;
using System.Collections.Generic;
using System.Linq;
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
                Logger.log.Error(e);
            }
        }

        public void CreateMaterialLights(GameObject go)
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
                    colorBloomLight.LightDataListener = ml;

                    colorBloomLight.color = ml.color;
                    ReflectionUtil.SetPrivateField(colorBloomLight, typeof(BSLight), "_ID", (int)ml.lightsID);

                    mbppLights.Add(colorBloomLight);
                    materialLightDescriptors.Add(ml);
                }
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }
    }
}
