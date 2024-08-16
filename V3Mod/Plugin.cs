using BepInEx;
using HarmonyLib;
using System.IO;
using UnityEngine.AddressableAssets;

namespace V3Mod
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static string modDir;

        private void Awake()
        {
            new Harmony(ConstInfo.GUID + ".harmony").PatchAll();
            Logger.LogInfo($"Plugin {ConstInfo.GUID} v{ConstInfo.VERSION} is loaded!");
            modDir = Path.GetDirectoryName(Info.Location);
            Addressables.LoadContentCatalogAsync(Path.Combine(modDir, "catalog.json")).WaitForCompletion();
        }
    }

    public static class ConstInfo
    {
        public const string GUID = $"JadeH.ULTRAKILL.{NAME}";
        public const string NAME = "V3Mod";
        public const string VERSION = "0.2.1";
    }
}