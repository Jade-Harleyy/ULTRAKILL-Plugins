using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CGEnemyIcons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static AssetBundle Assets = AssetBundle.LoadFromMemory(Properties.Resources.Assets);

        private void Awake()
        {
            Settings.Initialize();

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
