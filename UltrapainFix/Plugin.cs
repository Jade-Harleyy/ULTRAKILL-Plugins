using BepInEx;
using HarmonyLib;

namespace UltrapainMenuFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.eternalUnion.ultraPain", BepInDependency.DependencyFlags.HardDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
