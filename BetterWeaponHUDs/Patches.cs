using HarmonyLib;
using SettingsMenu.Components.Pages;
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
        public static TMP_Text HardDamageText { get; private set; }

        public static GameObject CrosshairRailcannonSlider { get; private set; }
        public static Transform CrosshairFistIconFill { get; private set; }
        public static GameObject CrosshairWeaponIcon { get; private set; }

        [HarmonyPatch(typeof(HudController), nameof(HudController.Start)), HarmonyPrefix]
        private static void HudController_Start(HudController __instance)
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
                HardDamageText = HardDamageNumber.GetComponent<TMP_Text>();
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
                powerUpMeter.sizeDelta = new Vector2(50f, 50f);
                powerUpMeter.GetComponent<Image>().fillClockwise = true;
                powerUpMeter.GetComponent<Image>().fillOrigin = 4;
            }

            if (!CrosshairFistIconFill && HudController.Instance)
            {
                GameObject crosshairFistObj = Plugin.Assets.LoadAsset<GameObject>("Fist Icon").Instantiate(__instance.transform, false);
                FistControl.Instance.fistPanels = FistControl.Instance.fistPanels.AddToArray(crosshairFistObj);
                crosshairFistObj.GetComponent<CopyImage>().imgToCopy = HudController.Instance.fistBackground;
                (CrosshairFistIconFill = crosshairFistObj.Find("Fill")).GetComponent<CopyImage>().imgToCopy = HudController.Instance.fistFill;
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

        [HarmonyPatch(typeof(HUDSettings), nameof(HUDSettings.OnPrefChanged)), HarmonyPostfix]
        private static void HUDSettings_OnPrefChanged(string key, object value)
        {
            if (key == "weaponIcons" && (bool)value) { FixSpeedometer(); }
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
            if (!CrosshairFistIconFill || !HudController.Instance) { return; }
            CrosshairFistIconFill.GetComponent<Image>().fillAmount = HudController.Instance.fistFill.fillAmount;
        }
        
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

        [HarmonyPatch(typeof(HealthBar), nameof(HealthBar.Update)), HarmonyPrefix]
        private static bool HealthBar_Update(HealthBar __instance)
        {
            if (!Settings.InstantHealthUpdate) { return true; }
            
            __instance.afterImageSliders?.Do(afterImageSlider => afterImageSlider.gameObject.SetActive(false));
            __instance.hpSliders?.Do(hpSlider => hpSlider.value = NewMovement.Instance.hp);
            if (__instance.antiHpSlider)
            {
                __instance.antiHpSlider.value = NewMovement.Instance.antiHp;
                if (__instance.antiHpSliderFill) { __instance.antiHpSliderFill.enabled = __instance.antiHpSlider.value > 0f; }
            }
            if (__instance.hpText)
            {
                if (__instance.antiHpText)
                {
                    __instance.hpText.text = '/' + __instance.difficulty == 0 ? __instance.hpText.text = "200" : (100 - Mathf.RoundToInt(NewMovement.Instance.antiHp)).ToString();
                }
                else
                {
                    __instance.hpText.text = NewMovement.Instance.hp.ToString();
                    if (__instance.changeTextColor)
                    {
                        __instance.hpText.color =
                            NewMovement.Instance.hp switch
                            {
                                <= 30 => Color.red,
                                <= 50 when __instance.yellowColor => Color.yellow,
                                _ => __instance.normalTextColor
                            };
                    }
                    else if (__instance.normalTextColor == Color.white)
                    {
                        __instance.hpText.color = NewMovement.Instance.hp <= 30
                            ? Color.red
                            : ColorBlindSettings.Instance.GetHudColor(HudColorType.healthText);
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(StaminaMeter), nameof(StaminaMeter.Update)), HarmonyPrefix]
        private static bool StaminaMeter_Update(StaminaMeter __instance)
        {
            if (!Settings.InstantStaminaUpdate) { return true; }
            
            if (!__instance.alwaysUpdate && (!__instance.parentCanvas || !__instance.parentCanvas.enabled)) { return false; }
            if (__instance.stm)
            {
                __instance.stm.value = NewMovement.Instance.boostCharge;
                if (__instance.stm.value >= __instance.stm.maxValue)
                {
                    if (!__instance.full) { __instance.Flash(); }
                    __instance.staminaBar.color = __instance.origColor;
                    __instance.full = true;
                }
                else
                {
                    __instance.staminaBar.color = __instance.emptyColor;
                    __instance.full = false;
                }
                if (__instance.flashColor.a > 0f)
                {
                    __instance.flashColor.a = Mathf.MoveTowards(__instance.flashColor.a, 0f, Time.deltaTime);
                    __instance.staminaFlash.color = __instance.flashColor;
                }
            }
            if (__instance.stmText)
            {
                __instance.stmText.text = (NewMovement.Instance.boostCharge / 100f).ToString("0.00");
                if (__instance.changeTextColor)
                {
                    __instance.stmText.color = NewMovement.Instance.boostCharge < 100f
                        ? Color.red
                        : MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.stamina);
                }
                else if (__instance.normalTextColor == Color.white)
                {
                    __instance.stmText.color = NewMovement.Instance.boostCharge < 100f
                        ? Color.red
                        : MonoSingleton<ColorBlindSettings>.Instance.GetHudColor(HudColorType.healthText);
                }
            }
            return false;
        }
    }
}