using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CGEnemyIcons
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static string ModDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly AssetBundle Assets = AssetBundle.LoadFromFile(Path.Combine(ModDir, "cgenemyicons.bundle"));

        private void Awake()
        {
            Settings.Initialize();

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
