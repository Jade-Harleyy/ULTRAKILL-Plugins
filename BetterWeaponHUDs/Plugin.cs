using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JadeLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterWeaponHUDs
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private static string ModDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly AssetBundle Assets = AssetBundle.LoadFromFile(Path.Combine(ModDir, "betterweaponhuds.bundle"));

        private static GameObject FUPIndicator;

        private void Awake()
        {
            Settings.Initialize(this);
            SceneManager.sceneLoaded += OnSceneLoaded;

            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update() => FUPIndicator?.SetActive(Settings.FUPAlert && HookArm.Instance is { returning: true } hookArm && hookArm.caughtObjects.Any(rb => rb && rb.TryGetComponent<Grenade>(out _)));

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!FUPIndicator && GameObject.Find("/Canvas") is { transform: { } canvasTF })
            {
                FUPIndicator = Assets.LoadAsset<GameObject>("FUP Indicator").Instantiate(canvasTF, false);
            }
        }
    }
}