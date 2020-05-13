using BS_Utils.Utilities;
using ReactiveMaterial.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReactiveMaterial
{
    public class MaterialLight : MonoBehaviour, ILightDataListener
    {
        public enum LightsID { Static = 0, BackLights = 1, BigRingLights = 2, LeftLasers = 3, RightLasers = 4, TrackAndBottom = 5 }
        public enum ColorType { None = 0, LeftColor = 1, RightColor = 2}

        [Tooltip("Default Color")]
        public Color color = Color.black;

        [Tooltip("Sets the BaseColor to Custom Colors")]
        public ColorType CustomColor = ColorType.None;

        public LightsID lightsID = LightsID.Static;

        public List<Material> materials;

        private LightWithIdManager _lightManager;

        private LightWithIdManager lightManager
        {
            get
            {
                if (_lightManager == null)
                    _lightManager = BeatSaberSearching.FindLightWithIdManager(BeatSaberSearching.GetCurrentEnvironment());
                return _lightManager;
            }
            set
            {
                _lightManager = value;
            }
        }

        void Start()
        {
            try
            {
                if (CustomColor == ColorType.None)
                    return;
                var colorManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();
                if (colorManager == null) return;

                var leftColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_saberAColor");
                var rightColor = ReflectionUtil.GetPrivateField<SimpleColorSO>(colorManager, "_saberBColor");
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
                Logger.log.Error(e);
            }
        }


        private void OnEnable()
        {
            BSEvents.menuSceneLoaded += SetColorToDefault;
            BSEvents.menuSceneLoadedFresh += SetColorToDefault;
            BSEvents.beatmapEvent += OnBeatmapEvent;
            BSEvents.gameSceneActive += OnGameScene;
            SetColorToDefault();
        }

        private void OnDisable()
        {
            BSEvents.menuSceneLoaded -= SetColorToDefault;
            BSEvents.menuSceneLoadedFresh -= SetColorToDefault;
            BSEvents.beatmapEvent -= OnBeatmapEvent;
            BSEvents.gameSceneActive -= OnGameScene;
        }

        private void OnGameScene()
        {
            lightManager = BeatSaberSearching.FindLightWithIdManager(BeatSaberSearching.GetCurrentEnvironment());
        }

        private void OnBeatmapEvent(BeatmapEventData obj)
        {
            int type = (int)obj.type + 1;
            if (type == (int)lightsID)
            {
                Color color = lightManager.GetColorForId(type) * 0.9f;
                OnColorChanged(color);
            }
        }

        public void OnColorChanged(Color color)
        {
            try
            {
                //Logger.log.Info("Color Set : " + color);
                foreach (Material mat in materials)
                {
                    mat.color = color;
                    mat.SetFloat("_Glow", color.a);
                }
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }

        private void SetColorToDefault()
        {
            try
            {
                OnColorChanged(this.color);
            }
            catch (Exception e)
            {
                Logger.log.Error(e);
            }
        }

    }
    
}
