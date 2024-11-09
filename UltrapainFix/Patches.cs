using HarmonyLib;
using UnityEngine;

namespace UltrapainMenuFix
{
    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(Ultrapain.Plugin), nameof(Ultrapain.Plugin.OnSceneChange)), HarmonyPostfix]
        private static void Plugin_OnSceneChange()
        {
            // Can't use null conditional here (Unity sucks).
            if (Ultrapain.Plugin.currentDifficultyButton == null || Ultrapain.Plugin.currentDifficultyButton.transform is not RectTransform buttonTransform) { return; }
            buttonTransform.anchoredPosition = new Vector2(20f, -200f);
            Transform parentTransform = buttonTransform.parent;
            for (int i = 7; i <= 13; i++)
            {
                parentTransform.GetChild(i).localPosition += new Vector3(0f, i >= 11 ? 52f : 26f, 0f);
            }
        }
    }
}
