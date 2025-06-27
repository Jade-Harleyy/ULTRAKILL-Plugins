using HarmonyLib;
using UnityEngine;

namespace ULTRAPhysics
{
    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(PlayerMovementParenting), nameof(PlayerMovementParenting.DetachPlayer)), HarmonyPostfix]
        private static void PlayerMovementParenting_DetachPlayer(Transform other, PlayerMovementParenting __instance)
        {
            if (__instance.lockParent) { return; }
            __instance.rb.AddForce(__instance.currentDelta / Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}