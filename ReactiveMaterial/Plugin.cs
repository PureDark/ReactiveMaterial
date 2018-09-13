using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveMaterial
{
    class Plugin : IPlugin
    {
        public string Name => "ReactiveMaterials";
        public string Version => "0.0.1";

        public void OnApplicationStart()
        {
        }

        public void OnApplicationQuit()
        {
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
        }

        private void SceneManagerSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}
