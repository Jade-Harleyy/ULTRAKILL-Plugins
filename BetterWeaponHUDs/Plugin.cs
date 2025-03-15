using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterWeaponHUDs
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static string ModDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly AssetBundle Assets = AssetBundle.LoadFromFile(Path.Combine(ModDir, "betterweaponhuds.bundle"));

        private GameObject FUPIndicator;

        private void Awake()
        {
            Settings.Initialize();
            SceneManager.sceneLoaded += OnSceneLoaded;

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            FUPIndicator?.SetActive(Settings.FUPAlert && HookArm.Instance is { caughtGrenade: not null, returning: true });
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!FUPIndicator && GameObject.Find("/Canvas") is { transform: Transform canvasTF })
            {
                FUPIndicator = Instantiate(Assets.LoadAsset<GameObject>("FUP Indicator"), canvasTF, false);
            }
        }
    }
}