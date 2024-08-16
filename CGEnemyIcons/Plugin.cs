using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CGEnemyIcons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle assets;

        private void Awake()
        {
            assets = AssetBundle.LoadFromMemory(Properties.Resources.assets);
            CGEnemyIcons.Config.Init();

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
