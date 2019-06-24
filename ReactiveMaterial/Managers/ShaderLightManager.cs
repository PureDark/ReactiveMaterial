using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveMaterial
{
    public class ShaderLightManager : MonoBehaviour
    {
        private static ShaderLightManager _instance;

        public static ShaderLightManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ShaderLightManager();
                return _instance;
            }
        }

        public enum LightsID { Static = 0, BackLights = 1, BigRingLights = 2, LeftLasers = 3, RightLasers = 4, TrackAndBottom = 5 }

        private List<BloomPrePassLight> bppLights;
        private List<ShaderLightDataListener> lightDataListeners;

        public class ShaderLightDataListener : ILightDataListener
        {
            public LightsID lightsID = LightsID.Static;
            private static float lastTime = 0;
            private float lastAlpha = 0;

            public ShaderLightDataListener(LightsID lightsID)
            {
                this.lightsID = lightsID;
            }

            public void OnColorChanged(Color color)
            {
                try
                {
                    string name = "_LightColor" + (int)lightsID;
                    Shader.SetGlobalColor(name, GammaCorrection(color));
                    if ((color.a < lastAlpha) && Time.time - lastTime > 0.1f)
                    {
                        lastTime = Time.time;
                        lastAlpha = 0;
                        Shader.SetGlobalColor("_LastColor", GammaCorrection(color));
                        //Console.WriteLine("OnColorChanged : _LastColor = {0}", Shader.GetGlobalColor("_LastColor"));
                    }
                    else
                    {
                        lastAlpha = color.a;
                    }
                }
                catch (Exception e)
                {
                    Logger.log.Error(e);
                }
            }
        }

        public static Color GammaCorrection(Color color)
        {
            color.r = (float)Math.Pow(color.r, 2.2);
            color.g = (float)Math.Pow(color.g, 2.2);
            color.b = (float)Math.Pow(color.b, 2.2);
            return color;
        }

        public void CreateLightDataForShader(GameObject go)
        {
            try
            {
                if (bppLights == null) bppLights = new List<BloomPrePassLight>();
                if (lightDataListeners == null) lightDataListeners = new List<ShaderLightDataListener>();

                for (int i = 1; i < 6; i++)
                {
                    var lightDataListener = new ShaderLightDataListener((LightsID)i);

                    ColorBloomPrePassLight colorBloomLight;
                    colorBloomLight = go.AddComponent<ColorBloomPrePassLight>();
                    colorBloomLight.Init();
                    colorBloomLight.LightDataListener = lightDataListener;
                    ReflectionUtil.SetPrivateField(colorBloomLight, typeof(BSLight), "_ID", i);

                    bppLights.Add(colorBloomLight);
                    lightDataListeners.Add(lightDataListener);
                }

                var colorManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();
                if (colorManager == null) return;

                var leftColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_colorA");
                var rightColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_colorB");
                Shader.SetGlobalColor("_LeftColor", GammaCorrection(leftColor.color));
                Shader.SetGlobalColor("_RightColor", GammaCorrection(rightColor.color));
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }
    }
}
