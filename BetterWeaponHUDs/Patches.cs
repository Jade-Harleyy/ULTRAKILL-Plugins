using HarmonyLib;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

namespace BetterWeaponHUDs
{
    [HarmonyPatch]
    internal static class GunDisplayPatches
    {
        [HarmonyPatch(typeof(HudController), "Awake"), HarmonyPostfix]
        private static void Awake(HudController __instance) => __instance.weaponIcon.transform.SetParent(Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("Gun Canvas"), __instance.transform, false).transform, false);

        [HarmonyPatch(typeof(HudController), "Start"), HarmonyPostfix]
        private static void Start(HudController __instance)
        {
            if (!__instance.speedometer.rect) return;
            __instance.speedometer.rect.anchoredPosition = new(-79, 190);
        }

        [HarmonyPatch(typeof(HudController), "CheckSituation"), HarmonyPostfix]
        private static void CheckSituation(HudController __instance) => __instance.weaponIcon?.transform?.parent?.gameObject?.SetActive(!HideUI.Active && PrefsManager.Instance.GetInt("hudType") == 1);

        [HarmonyPatch(typeof(HUDOptions), "WeaponIcon"), HarmonyPostfix]
        private static void WeaponIcon(HudController[] ___hudCons) => ___hudCons.DoIf(hudCon => hudCon.speedometer.rect, hudCon => hudCon.speedometer.rect.anchoredPosition = new(-79, 190));

        [HarmonyPatch(typeof(RailcannonMeter), "CheckStatus"), HarmonyPostfix]
        private static void RailcannonMeter_CheckStatus(RailcannonMeter __instance)
        {
            if (!PrefsManager.Instance.GetBool("weaponIcons") || HudController.Instance.altHud) return;

            Traverse.Create(__instance).Field("self").GetValue<Image>().enabled = false;
            __instance.miniVersion.SetActive(true);
        }
    }

    [HarmonyPatch]
    internal static class HPDisplayPatches
    {
        [HarmonyPatch(typeof(HudController), "Awake"), HarmonyPostfix]
        private static void Awake(HudController __instance) => __instance.textElements = __instance.textElements.AddToArray(Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("Hard Damage"), __instance.transform.Find("GunCanvas/StatsPanel/Filler/Panel (2)/Filler"), false).GetComponent<TMP_Text>());
    }
}