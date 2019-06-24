using IPALogger = IPA.Logging.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPA;

namespace ReactiveMaterial
{
    class Plugin : IBeatSaberPlugin
    {
        public string Name => "ReactiveMaterial";
        public string Version => "0.1.0";

        public void Init(IPALogger logger)
        {
            Logger.log = logger;
        }

        public void OnApplicationStart()
        {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
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
