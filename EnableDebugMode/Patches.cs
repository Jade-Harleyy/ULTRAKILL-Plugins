using HarmonyLib;
using UnityEngine;

namespace EnableDebugMode
{
    [HarmonyPatch(typeof(Debug))]
    internal static class Patches
    {
        [HarmonyPatch("isDebugBuild", MethodType.Getter), HarmonyPostfix]
        private static void get_isDebugBuild(ref bool __result) => __result = true;
    }
}