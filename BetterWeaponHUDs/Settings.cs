using BepInEx;
using JadeLib;
using JadeLib.PluginConfigurator.Fields;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BetterWeaponHUDs
{
    public static class Settings
    {
        private static readonly string[] validExtensions = [".png", ".jpg", ".exf"];

        #region HUD
        private static BoolField hudAcceleration;
        public static bool HUDAcceleration => hudAcceleration.value;

        #region Crosshair HUD
        private static BoolField useAlternateCrosshair;
        public static bool UseAlternateCrosshair => useAlternateCrosshair.value;

        private static BoolField showRailcannonCharge;
        public static bool ShowRailcannonCharge => showRailcannonCharge.value && RailcannonMeter.Instance.RailcannonStatus();
        #endregion

        #region Status HUD
        private static BoolField showHardDamageNumber;
        public static bool ShowHardDamageNumber => showHardDamageNumber.value;

        private static BoolField altIndicatorPosition;
        public static bool AltIndicatorPosition => altIndicatorPosition.value;

        private static BoolField forceAltRailcannonCharge;
        public static bool ForceAltRailcannonCharge => forceAltRailcannonCharge.value;
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

            useAlternateCrosshair = new(config.rootPanel, "ALTERNATE CROSSHAIR", "use_alt_crosshair", false);
            useAlternateCrosshair.postValueChangeEvent += _ => HUDOptions.Instance?.crosshair.CheckCrossHair();

            showRailcannonCharge = new(config.rootPanel, "RAILCANNON CHARGE", "crosshair_rc_charge", false);
            #endregion

            #region Status HUD
            new ConfigHeader(config.rootPanel, "STATUS HUD");

            showHardDamageNumber = new(config.rootPanel, "HARD DAMAGE INDICATOR", "show_hard_damage", false);
            showHardDamageNumber.postValueChangeEvent += _ => HudController.Instance?.CheckSituation();

            altIndicatorPosition = new(config.rootPanel, "ALTERNATE WEAPON ICON POSITION", "alt_equipped_indicator_pos", false);
            altIndicatorPosition.postValueChangeEvent += value =>
            {
                forceAltRailcannonCharge.interactable = !value;
                RailcannonMeter.Instance?.CheckStatus();
                HudController.Instance?.CheckSituation();
            };

            forceAltRailcannonCharge = new(config.rootPanel, "FORCE ALTERNATE RAILCANNON DISPLAY", "force_alt_railcannon_charge", false) { interactable = !altIndicatorPosition.value };
            forceAltRailcannonCharge.postValueChangeEvent += _ => RailcannonMeter.Instance?.CheckStatus();
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
                    if (path.IsNullOrWhiteSpace())
                    {
                        prevValue = "";
                        spriteField.Sprite = null;
                    }
                    else
                    {
                        Texture2D texture = new(0, 0);
                        if (validExtensions.Any(path.EndsWith) && File.Exists(path) && texture.LoadImage(File.ReadAllBytes(path)))
                        {
                            prevValue = path;
                            spriteField.Sprite = Sprite.Create(texture, new(0f, 0f, texture.width, texture.height), new(0.5f, 0.5f));
                        }
                        else
                        {
                            stringField.value = prevValue;
                        }
                    }

                    CustomStyleImages[index] = spriteField.Sprite;
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
