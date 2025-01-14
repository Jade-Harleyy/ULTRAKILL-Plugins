using System.Linq;
using HarmonyLib;
using Train;
using UnityEngine;

namespace ULTRAPhysics
{
    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(PlayerMovementParenting), nameof(PlayerMovementParenting.DetachPlayer)), HarmonyPostfix]
        private static void PlayerMovementParenting_DetachPlayer(Transform other, Transform ___deltaReceiver)
        {
            if (other is null || ___deltaReceiver?.GetComponent<Rigidbody>() is not { } playerBody) { return; }
            if ((Resources.FindObjectsOfTypeAll<Tram>().FirstOrDefault(parentTram => parentTram.connectedTrams.Contains(other.GetComponentInParent<ConnectedTram>())) ?? other.GetComponentInParent<Tram>()) is not { } tram) { return; }
            playerBody.velocity += tram.transform.forward * tram.computedSpeed;
        }
    }
}