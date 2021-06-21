using BS_Utils.Utilities;
using ReactiveMaterial.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReactiveMaterial
{
    public class ShaderLight : MonoBehaviour
    {

        public enum LightsID { Static = 0, BackLights = 1, BigRingLights = 2, LeftLasers = 3, RightLasers = 4, TrackAndBottom = 5 }

        private ShaderLightDataListener[] lightDataListeners = new ShaderLightDataListener[6];

        private LightWithIdManager _lightManager;

        private LightWithIdManager lightManager
        {
            get {
                return _lightManager;
            }
            set
            {
                _lightManager = value;
            }
        }


        public class ShaderLightDataListener
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

        public void Start()
        {
            try
            {
                if (lightDataListeners == null)
                    lightDataListeners = new ShaderLightDataListener[6];

                for (int i = 1; i < 6; i++)
                {
                    lightDataListeners[i] = new ShaderLightDataListener((LightsID)i);
                }

                var colorManager = Resources.FindObjectsOfTypeAll<SaberModelController>().FirstOrDefault().GetPrivateField<ColorManager>("_colorManager");
                if (colorManager == null) return;

                var leftColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_saberAColor");
                var rightColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_saberBColor");
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }

        private void OnEnable()
        {
            //BSEvents.beatmapEvent += OnBeatmapEvent;
            BSEvents.gameSceneActive += OnGameScene;
        }

        private void OnDisable()
        {
            //BSEvents.beatmapEvent -= OnBeatmapEvent;
            BSEvents.gameSceneActive -= OnGameScene;
        }

        private void OnGameScene()
        {
            lightManager = BeatSaberSearching.FindLightWithIdManager(BeatSaberSearching.GetCurrentEnvironment());
        }

        private void Update()
        {
            try
            {
                if (lightManager == null)
                    return;
                for (int lightID = 1; lightID < 5; lightID++)
                {
                    Color color = lightManager.GetColorForId(lightID) * 0.9f;
                    lightDataListeners[lightID].OnColorChanged(color);
                }
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }

    }
    
}
