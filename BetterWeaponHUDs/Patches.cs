using HarmonyLib;
using TMPro;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.UI;

namespace BetterWeaponHUDs
{
    [HarmonyPatch]
    internal static class Patches
    {
        private static readonly Vector2 speedometerPos = new(-79f, 590f);
        private static readonly Vector2 altSpeedometerPos = new(-79f, 190f);

        [HarmonyPatch(typeof(HudController), nameof(HudController.Awake)), HarmonyPostfix]
        private static void HudController_Awake(HudController __instance)
        {
            Object.Instantiate(Plugin.Assets.LoadAsset<GameObject>("Gun Canvas"), __instance.transform, false);
            __instance.textElements = __instance.textElements.AddToArray(Object.Instantiate(Plugin.Assets.LoadAsset<GameObject>("Hard Damage"), __instance.transform.Find("GunCanvas/StatsPanel/Filler/Panel (2)/Filler"), false).GetComponent<TMP_Text>());
        }

        [HarmonyPatch(typeof(HudController), nameof(HudController.CheckSituation)), HarmonyPostfix]
        private static void HudController_CheckSituation(HudController __instance)
        {
            if (__instance.altHud) { return; }
            if (__instance.speedometer.rect is RectTransform rect && PrefsManager.Instance.GetBool("weaponIcons")) { rect.anchoredPosition = Settings.AltIndicatorPosition ? altSpeedometerPos : speedometerPos; }

            Transform altCanvas = __instance.transform.Find("Gun Canvas(Clone)");
            altCanvas.gameObject.SetActive(!HideUI.Active && PrefsManager.Instance.GetInt("hudType") == 1);
            __instance.transform.Find("GunCanvas/StatsPanel/Filler/Panel (2)/Filler/Hard Damage(Clone)")?.gameObject.SetActive(Settings.ShowHardDamageNumber);
            __instance.weaponIcon.transform.SetParent(Settings.AltIndicatorPosition ? altCanvas : __instance.transform.Find("GunCanvas"), false);
        }

        [HarmonyPatch(typeof(RailcannonMeter), nameof(RailcannonMeter.CheckStatus)), HarmonyPostfix]
        private static void RailcannonMeter_CheckStatus(RailcannonMeter __instance)
        {
            if (!PrefsManager.Instance.GetBool("weaponIcons") || !Settings.AltIndicatorPosition && !Settings.ForceAltRailcannonCharge) { return; }

            __instance.self.enabled = false;
            __instance.miniVersion.SetActive(__instance.RailcannonStatus());
        }

        [HarmonyPatch(typeof(Crosshair), nameof(Crosshair.Start)), HarmonyPrefix]
        private static void Crosshair_Start(Crosshair __instance)
        {
            Object.Instantiate(Plugin.Assets.LoadAsset<GameObject>("Railcannon Slider"), __instance.transform.Find("PowerUpBar"), false);
            RectTransform powerUpMeter = __instance.transform.Find("PowerUpBar/HealthSliderAfterImage (2)") as RectTransform;
            powerUpMeter.sizeDelta = new Vector2(50f, 50f);
            powerUpMeter.GetComponent<Image>().fillClockwise = true;
            powerUpMeter.GetComponent<Image>().fillOrigin = 4;
        }

        [HarmonyPatch(typeof(Crosshair), nameof(Crosshair.CheckCrossHair)), HarmonyPostfix]
        private static void Crosshair_CheckCrossHair(Crosshair __instance)
        {
            if (!Settings.UseAlternateCrosshair) { return; }

            __instance.mainch.enabled = false;
            foreach (Image image in __instance.altchs)
            {
                image.enabled = true;
            }
        }

        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.Update)), HarmonyPostfix]
        private static void StatsManager_Update(StatsManager __instance)
        {
            if (__instance.crosshair?.transform.Find("PowerUpBar/Railcannon Slider(Clone)")?.GetComponent<Image>() is not Image rcSlider) { return; }
            if (!(rcSlider.enabled = Settings.ShowRailcannonCharge)) { return; }

            rcSlider.fillAmount = Mathf.Lerp(0f, 0.5f, WeaponCharges.Instance.raicharge / 4f);
            rcSlider.color = WeaponCharges.Instance.raicharge >= 5f ? ColorBlindSettings.Instance.railcannonFullColor : ColorBlindSettings.Instance.railcannonChargingColor;
        }

        [HarmonyPatch(typeof(PowerUpMeter), nameof(PowerUpMeter.UpdateMeter)), HarmonyPostfix]
        private static void PowerUpMeter_UpdateMeter(PowerUpMeter __instance)
        {
            if (!Settings.ShowRailcannonCharge) { return; }

            __instance.meter.fillAmount /= 2f;
        }

        [HarmonyPatch(typeof(HUDOptions), nameof(HUDOptions.WeaponIcon)), HarmonyPostfix]
        private static void HUDOptions_WeaponIcon(bool stuff)
        {
            if (HudController.Instance?.speedometer.rect is RectTransform rect && stuff) { rect.anchoredPosition = Settings.AltIndicatorPosition ? altSpeedometerPos : speedometerPos; }
        }

        [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.rankIndex), MethodType.Setter), HarmonyPostfix]
        private static void StyleHUD_set_rankIndex(StyleHUD __instance) => SetRankImage(__instance);

        [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.DescendRank)), HarmonyPostfix]
        private static void StyleHUD_DescendRank(StyleHUD __instance) => SetRankImage(__instance);

        private static void SetRankImage(StyleHUD __instance)
        {
            if (Settings.CustomStyleImages.Length <= __instance.rankIndex || Settings.CustomStyleImages[__instance.rankIndex] is not Sprite sprite) { return; }
            __instance.rankImage.sprite = sprite;
        }
    }
}