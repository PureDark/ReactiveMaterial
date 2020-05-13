using System;
using System.Linq;
using UnityEngine;

namespace ReactiveMaterial
{
    public class ShaderLight : MonoBehaviour
    {

        public enum LightsID { Static = 0, BackLights = 1, BigRingLights = 2, LeftLasers = 3, RightLasers = 4, TrackAndBottom = 5 }

        private ShaderLightDataListener[] lightDataListeners = new ShaderLightDataListener[6];


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
        }
        
    }
    
}
