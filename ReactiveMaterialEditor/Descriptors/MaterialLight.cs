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

        void Start()
        {
        }


        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnGameScene()
        {
        }

        public void OnColorChanged(Color color)
        {
        }
    }
    
}
