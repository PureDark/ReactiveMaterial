using IPALogger = IPA.Logging.Logger;
using IPA;
using BS_Utils.Utilities;

namespace ReactiveMaterial
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    class Plugin
    {

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
