using JadeLib;
using JadeLib.PluginConfigurator.Fields;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using UnityEngine;

namespace BetterWeaponHUDs
{
    public static class Settings
    {
        #region HUD
        private static BoolField hudAcceleration;
        public static bool HUDAcceleration => hudAcceleration.value;

        #region Crosshair HUD
        private static BoolField alternateCrosshair;
        public static bool AlternateCrosshair => alternateCrosshair.value;

        private static BoolField crosshairRailcannonCharge;
        public static bool CrosshairRailcannonCharge => crosshairRailcannonCharge.value && RailcannonMeter.Instance.RailcannonStatus();

        private static BoolField crosshairFistIcon;
        public static bool CrosshairFistIcon => crosshairFistIcon.value;

        private static BoolField crosshairWeaponIcon;
        public static bool CrosshairWeaponIcon => crosshairWeaponIcon.value;
        #endregion

        #region Status HUD
        private static BoolField hardDamageNumber;
        public static bool HardDamageNumber => hardDamageNumber.value;

        private static BoolField altIndicatorPosition;
        public static bool AltIndicatorPosition => altIndicatorPosition.value;

        private static BoolField forceAltRailcannonCharge;
        public static bool ForceAltRailcannonCharge => forceAltRailcannonCharge.value;

        private static BoolField fistCooldown;
        public static bool FistCooldown => fistCooldown.value;
        #endregion

        #region Style HUD
        public static readonly Sprite[] CustomStyleImages = new Sprite[8];
        #endregion

        #region Other
        private static BoolField fupAlert;
        public static bool FUPAlert => fupAlert.value;
        #endregion
        #endregion

        #region Viewmodel
        private static BoolField walkingBob;
        public static bool WalkingBob => walkingBob.value;

        private static BoolField viewmodelAcceleration;
        public static bool ViewmodelAcceleration => viewmodelAcceleration.value;
        #endregion

        internal static void Initialize()
        {
            PluginConfigurator config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.icon = Plugin.Assets.LoadAsset<Sprite>("Icon");

            #region HUD
            hudAcceleration = new(config.rootPanel, "HUD FOLLOWS SPEED", "hudacceleration", true);

            #region Crosshair HUD
            new ConfigHeader(config.rootPanel, "CROSSHAIR HUD");

            alternateCrosshair = new(config.rootPanel, "ALTERNATE CROSSHAIR", "use_alt_crosshair", false);
            alternateCrosshair.postValueChangeEvent += _ => HUDOptions.Instance?.crosshair.CheckCrossHair();

            crosshairRailcannonCharge = new(config.rootPanel, "RAILCANNON CHARGE", "crosshair_rc_charge", false);
            crosshairRailcannonCharge.postValueChangeEvent += _ => Patches.CrosshairRailcannonSlider?.SetActive(CrosshairRailcannonCharge);

            crosshairFistIcon = new(config.rootPanel, "FIST ICON", "crosshair_fist_icon", false);
            crosshairFistIcon.postValueChangeEvent += value => Patches.CrosshairFistIcon?.parent.SetActive(value);

            crosshairWeaponIcon = new(config.rootPanel, "WEAPON ICON", "crosshair_weapon_icon", false);
            crosshairWeaponIcon.postValueChangeEvent += value => Patches.CrosshairWeaponIcon?.SetActive(value);
            #endregion

            #region Status HUD
            new ConfigHeader(config.rootPanel, "STATUS HUD");

            hardDamageNumber = new(config.rootPanel, "HARD DAMAGE INDICATOR", "show_hard_damage", false);
            hardDamageNumber.postValueChangeEvent += value => Patches.HardDamageNumber?.SetActive(value);

            altIndicatorPosition = new(config.rootPanel, "ALTERNATE WEAPON ICON POSITION", "alt_equipped_indicator_pos", false);
            altIndicatorPosition.postValueChangeEvent += value =>
            {
                forceAltRailcannonCharge.interactable = !value;
                Patches.SetIconParent(value);
            };

            forceAltRailcannonCharge = new(config.rootPanel, "FORCE ALTERNATE RAILCANNON DISPLAY", "force_alt_railcannon_charge", false) { interactable = !altIndicatorPosition.value };
            forceAltRailcannonCharge.postValueChangeEvent += _ => RailcannonMeter.Instance?.CheckStatus();

            fistCooldown = new(config.rootPanel, "SHOW FIST COOLDOWN", "show_fist_cooldown", false);
            #endregion

            #region Style HUD
            new ConfigHeader(config.rootPanel, "STYLE HUD");

            new ConfigHeader(config.rootPanel, "CUSTOM STYLE RANK IMAGES", 16);
            new ConfigHeader(config.rootPanel, "MUST BE THE FULL PATH TO A .PNG .JPG OR .EXF FILE\nLEAVE BLANK TO USE DEFAULT IMAGE", 12);
            for (int i = 0; i < 8; i++)
            {
                string prevValue = "";
                int index = i;

                StringField stringField = new(config.rootPanel, GenericExtensions.GetRankText(i), "customrankimages_" + i, "", true);
                SpriteField spriteField = new(config.rootPanel, 100f);
                stringField.postValueChangeEvent += TrySetSprite;
                TrySetSprite(stringField.value);

                void TrySetSprite(string path)
                {
                    Sprite sprite = ImageImporter.LoadFromFile(path, out ImageImporter.Error error);
                    switch (error)
                    {
                        case ImageImporter.Error.NoError:
                        case ImageImporter.Error.PathNullOrEmpty:
                            prevValue = path ?? "";
                            break;
                        case ImageImporter.Error.FileMissing:
                        case ImageImporter.Error.FailedToLoad:
                            stringField.value = prevValue;
                            return;
                    }

                    CustomStyleImages[index] = spriteField.Sprite = sprite;
                    if (StyleHUD.Instance is not StyleHUD styleHUD || index == styleHUD.rankIndex) { return; }
                    styleHUD.rankImage.sprite = spriteField.Sprite ?? styleHUD.ranks[index].sprite;
                }
            }
            #endregion

            #region Other
            new ConfigHeader(config.rootPanel, "OTHER");
            fupAlert = new(config.rootPanel, "ROCKET WHIPLASH ALERT", "fup_alert", false);
            #endregion
            #endregion

            #region Viewmodel
            new ConfigHeader(config.rootPanel, "VIEWMODEL");

            walkingBob = new(config.rootPanel, "WALKING BOB", "walkingbob", true);
            walkingBob.postValueChangeEvent += value =>
            {
                if (NewMovement.Instance?.GetComponentInChildren<WalkingBob>(true) is not WalkingBob walkingBob) { return; }
                walkingBob.enabled = value;
                walkingBob.transform.localPosition = Vector3.zero;
            };

            viewmodelAcceleration = new(config.rootPanel, "WEAPONS FOLLOW SPEED", "viewmodelacceleration", true);
            #endregion
        }
    }
}
