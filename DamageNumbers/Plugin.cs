using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DamageNumbers
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static string ModDir => Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
        public static readonly AssetBundle Assets = AssetBundle.LoadFromFile($"{ModDir}/damagenumbers.bundle");

        private void Awake()
        {
            Settings.Initialize();
            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}