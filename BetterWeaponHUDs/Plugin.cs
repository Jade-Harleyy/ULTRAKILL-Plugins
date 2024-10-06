using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BetterWeaponHUDs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static readonly AssetBundle Assets = AssetBundle.LoadFromMemory(Properties.Resources.Assets);

        private void Awake()
        {
            Settings.Initialize();

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}