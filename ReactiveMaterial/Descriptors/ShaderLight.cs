using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveMaterial
{
    public class ShaderLight : MonoBehaviour
    {

        void Start()
        {
            try
            {
                
                if (gameObject.GetComponentInChildren<ShaderLightManager>(true) == null)
                {
                    ShaderLightManager slm = gameObject.AddComponent<ShaderLightManager>();
                    slm.CreateLightDataForShader(gameObject);
                }
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

    }
    
}
