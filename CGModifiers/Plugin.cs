using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CGModifiers
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private const string CGSceneName = "9240e656c89994d44b21940f65ab57da";
        private const string FirstRoomPath = "/FirstRoom/Room";
        private const string WavePanelPath = $"{FirstRoomPath}/CyberGrindSettings/Canvas/Waves/Panel";

        public static AssetBundle assets;

        private void Awake()
        {
            assets = AssetBundle.LoadFromMemory(Properties.Resources.assets);

            SceneManager.sceneLoaded += OnSceneLoaded;

            new Harmony(PluginInfo.PLUGIN_GUID + ".harmony").PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != CGSceneName) return;

            Transform wavePanelTF = GameObject.Find(WavePanelPath).transform;
            GameObject waveSelector = Instantiate(assets.LoadAsset<GameObject>("Wave Selector"), wavePanelTF, false);

            Transform text = wavePanelTF.Find("Text");
            text.localPosition += new Vector3(0, 10, 0);
            text.GetComponent<TextMeshProUGUI>().paragraphSpacing = -30;

            wavePanelTF.Find("0").GetComponent<WaveSetter>().wave = 1;

            Instantiate(assets.LoadAsset<GameObject>("Modifiers Terminal"), GameObject.Find(FirstRoomPath).transform, false).GetComponent<ModifierManager>().waveSelect = waveSelector.GetComponent<FloatSelector>();
        }
    }
}