using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace EnableDebugMode
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony(ConstInfo.GUID + ".harmony").PatchAll();
            Logger.LogInfo($"Plugin {ConstInfo.GUID} v{ConstInfo.VERSION} is loaded!");
        }
    }

    public static class ConstInfo
    {
        public const string GUID    = "JadeH.ULTRAKILL.EnableDebugMode";
        public const string NAME    = "EnableDebugMode";
        public const string VERSION = "1.0.0";
    }

    [HarmonyPatch(typeof(Debug))]
    internal class Patch
    {
        [HarmonyPatch("isDebugBuild", MethodType.Getter), HarmonyPostfix]
        private static void get_isDebugBuild(ref bool __result)
        {
            __result = true;
        }
    }
}