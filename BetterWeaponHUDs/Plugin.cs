using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterWeaponHUDs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static readonly AssetBundle Assets = AssetBundle.LoadFromMemory(Properties.Resources.Assets);

        private GameObject FUPIndicator;

        private void Awake()
        {
            Settings.Initialize();
            SceneManager.sceneLoaded += OnSceneLoaded;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update() => FUPIndicator?.SetActive(Settings.FUPAlert && HookArm.Instance is { caughtGrenade: not null, returning: true });

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!FUPIndicator && GameObject.Find("/Canvas")?.transform is Transform canvasTF)
            {
                FUPIndicator = Instantiate(Assets.LoadAsset<GameObject>("FUP Indicator"), canvasTF, false);
            }
        }
    }
}