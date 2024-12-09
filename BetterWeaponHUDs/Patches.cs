using HarmonyLib;
using JadeLib;
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

        private static GameObject AltGunCanvas { get; set; }
        public static GameObject HardDamageNumber { get; private set; }

        public static GameObject CrosshairRailcannonSlider { get; private set; }
        public static Transform CrosshairFistIcon { get; private set; }
        public static GameObject CrosshairWeaponIcon { get; private set; }

        private static float PunchLastMaxCooldown = 1f;

        [HarmonyPatch(typeof(HudController), nameof(HudController.Awake)), HarmonyPostfix]
        private static void HudController_Awake(HudController __instance)
        {
            if (__instance.altHud) { return; }

            if (!AltGunCanvas)
            {
                AltGunCanvas = Plugin.Assets.LoadAsset<GameObject>("Gun Canvas").Instantiate(__instance.transform, false);
                SetIconParent(Settings.AltIndicatorPosition);
            }

            if (!HardDamageNumber)
            {
                HardDamageNumber = Plugin.Assets.LoadAsset<GameObject>("Hard Damage").Instantiate(__instance.Find("GunCanvas/StatsPanel/Filler/Panel (2)/Filler"), false);
                __instance.textElements = __instance.textElements.AddToArray(HardDamageNumber.GetComponent<TMP_Text>());
                HardDamageNumber.SetActive(Settings.HardDamageNumber);
            }
        }

        [HarmonyPatch(typeof(HudController), nameof(HudController.CheckSituation)), HarmonyPostfix]
        private static void HudController_CheckSituation(HudController __instance)
        {
            if (__instance.altHud || !AltGunCanvas) { return; }
            AltGunCanvas.GetComponent<Canvas>().enabled = !HideUI.Active && PrefsManager.Instance.GetInt("hudType") == 1;
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
            if (!CrosshairRailcannonSlider)
            {
                CrosshairRailcannonSlider = Plugin.Assets.LoadAsset<GameObject>("Railcannon Slider").Instantiate(__instance.Find("PowerUpBar"), false);
                CrosshairRailcannonSlider.SetActive(Settings.CrosshairRailcannonCharge);

                RectTransform powerUpMeter = __instance.FindAsRect("PowerUpBar/HealthSliderAfterImage (2)");
                powerUpMeter.sizeDelta = new(50f, 50f);
                powerUpMeter.GetComponent<Image>().fillClockwise = true;
                powerUpMeter.GetComponent<Image>().fillOrigin = 4;
            }

            if (!CrosshairFistIcon)
            {
                GameObject crosshairFistObj = Plugin.Assets.LoadAsset<GameObject>("Fist Icon").Instantiate(__instance.transform, false);
                FistControl.Instance.fistPanels = FistControl.Instance.fistPanels.AddToArray(crosshairFistObj);
                (CrosshairFistIcon = crosshairFistObj.Find("Fill")).GetComponent<CopyImage>().imgToCopy = FistControl.Instance.fistIcon;
                crosshairFistObj.SetActive(Settings.CrosshairFistIcon);
            }

            if (!CrosshairWeaponIcon)
            {
                if (!WeaponHUD.Instance) { Object.FindObjectOfType<WeaponHUD>()?.Awake(); }
                CrosshairWeaponIcon = Plugin.Assets.LoadAsset<GameObject>("Weapon Icon").Instantiate(__instance.transform, false);
                CrosshairWeaponIcon.SetActive(Settings.CrosshairWeaponIcon);
            }
        }

        [HarmonyPatch(typeof(Crosshair), nameof(Crosshair.CheckCrossHair)), HarmonyPostfix]
        private static void Crosshair_CheckCrossHair(Crosshair __instance)
        {
            if (!Settings.AlternateCrosshair) { return; }
            __instance.mainch.enabled = false;
            __instance.altchs.Do(image => image.enabled = true);
        }

        [HarmonyPatch(typeof(PowerUpMeter), nameof(PowerUpMeter.UpdateMeter)), HarmonyPostfix]
        private static void PowerUpMeter_UpdateMeter(PowerUpMeter __instance)
        {
            if (!CrosshairRailcannonSlider || !Settings.CrosshairRailcannonCharge) { return; }

            Image rcSlider = CrosshairRailcannonSlider.GetComponent<Image>();
            rcSlider.fillAmount = Mathf.Lerp(0f, 0.5f, WeaponCharges.Instance.raicharge / 4f);
            rcSlider.color = WeaponCharges.Instance.raicharge >= 5f ? ColorBlindSettings.Instance.railcannonFullColor : ColorBlindSettings.Instance.railcannonChargingColor;

            __instance.meter.fillAmount /= 2f;
        }

        [HarmonyPatch(typeof(HUDOptions), nameof(HUDOptions.WeaponIcon)), HarmonyPostfix]
        private static void HUDOptions_WeaponIcon(bool stuff)
        {
            if (stuff) { FixSpeedometer(); }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.rankIndex), MethodType.Setter)]
        [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.DescendRank))]
        private static void SetRankImage(StyleHUD __instance)
        {
            if (Settings.CustomStyleImages.Length <= __instance.rankIndex || Settings.CustomStyleImages[__instance.rankIndex] is not { } sprite) { return; }
            __instance.rankImage.sprite = sprite;
        }

        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Update)), HarmonyPostfix]
        private static void NewMovement_Update(NewMovement __instance)
        {
            if (!Settings.ViewmodelAcceleration)
            {
                __instance.hudCam.transform.localPosition = __instance.camOriginalPos;
            }

            if (!Settings.HUDAcceleration)
            {
                __instance.screenHud.transform.localPosition = __instance.hudOriginalPos;
            }
        }

        [HarmonyPatch(typeof(WalkingBob), nameof(WalkingBob.Awake)), HarmonyPostfix]
        private static void WalkingBob_Awake(WalkingBob __instance) => __instance.enabled = Settings.WalkingBob;

        [HarmonyPatch(typeof(FistControl), nameof(FistControl.Update)), HarmonyPostfix]
        private static void FistControl_Update(FistControl __instance)
        {
            if (!CrosshairFistIcon) { return; }
            CrosshairFistIcon.GetComponent<Image>().fillAmount = Settings.FistCooldown ? 1f - __instance.fistCooldown / PunchLastMaxCooldown : 1f;
        }
        
        [HarmonyPatch(typeof(Punch), nameof(Punch.PunchStart)), HarmonyPostfix]
        private static void Punch_PunchStart(Punch __instance) => PunchLastMaxCooldown = __instance.fc.fistCooldown;
        
        [HarmonyPatch(typeof(StyleHUD), nameof(StyleHUD.GetLocalizedName)), HarmonyPostfix]
        private static void StyleHUD_GetLocalizedName(string id, string __result)
        {
            Settings.AddStyleBonusEntry(id, __result);
        }
        
        public static void SetIconParent(bool alt)
        {
            HudController.Instance?.weaponIcon.SetParent(alt ? AltGunCanvas : HudController.Instance.gunCanvas, false);
            RailcannonMeter.Instance?.CheckStatus();
            FixSpeedometer();
        }

        private static void FixSpeedometer()
        {
            if (!HudController.Instance) { return; }
            HudController.Instance.speedometer.rect.anchoredPosition = Settings.AltIndicatorPosition ? altSpeedometerPos : speedometerPos;
        }
    }
}