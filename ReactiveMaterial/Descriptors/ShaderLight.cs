using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ReactiveMaterial
{
    public class ShaderLight : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        public List<Material> materials;

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
                
                if (gameObject.GetComponentInChildren<ShaderLightManager>(true) == null)
                {
                    ShaderLightManager slm = gameObject.AddComponent<ShaderLightManager>();
                    slm.CreateLightDataForShader(gameObject);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

    }
    
}
