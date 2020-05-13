using IPALogger = IPA.Logging.Logger;
using UnityEngine.SceneManagement;
using IPA;
using BS_Utils.Utilities;

namespace ReactiveMaterial
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    class Plugin
    {
        public string Name => "ReactiveMaterial";
        public string Version => "0.3.0";

        [Init]
        public void Init(IPALogger logger)
        {
            Logger.log = logger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            BSEvents.OnLoad();
        }
    }
}
