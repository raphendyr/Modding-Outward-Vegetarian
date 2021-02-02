using BepInEx;
using Nm1fiOutward.Drops;
using SideLoader;
using SideLoader.SLPacks;
using System.Reflection;

namespace Nm1fiOutward.Vegetarian
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(SLPlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DropsPlugin.GUID, "0.1.0")] // TODO: set to ^1.0.0 after release and BepInEx support
    public class Vegetarian : BaseUnityPlugin
    {
        public const string GUID = "github.raphendyr.vegetarian";
        public const string NAME = "Vegetarian";
        public const string FEATURE_VERSION = "0.1";
        public const string VERSION = "0.1.0";

        // Resource object name is: <project default namespace>.SideLoader.zip
        private const string SIDELOADER_RESOURCE = "Nm1fiOutward.Vegetarian.SideLoader.zip";

        internal void Awake()
        {
            Logger.LogMessage($"Version {VERSION} loading...");

            var asm = Assembly.GetExecutingAssembly();
            var resourceNames = asm.GetManifestResourceNames();
            Logger.LogInfo("List of all bundled resources:\n" + string.Join("\n  ", resourceNames));

#if !DEBUG
            if (resourceNames.Contains(SIDELOADER_RESOURCE))
            {
                // Load SideLoader archive from DLL resources
                var resource = asm.GetManifestResourceStream(SIDELOADER_RESOURCE);
                SLPackArchive.CreatePackFromStream(resource, NAME);
            }
            else
            {
                Logger.LogWarning("Expected resource was not found from the DLL. Is this a development build?");
            }
#endif
        }
    }
}
