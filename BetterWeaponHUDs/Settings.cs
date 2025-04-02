using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using JadeLib;
using JadeLib.PluginConfigurator.Fields;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using UnityEngine;

namespace BetterWeaponHUDs
{
    internal static class Settings
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
        #endregion

        #region Style HUD
        public static readonly Sprite[] CustomStyleImages = new Sprite[8];

        private const string DefaultStyleGUID = MyPluginInfo.PLUGIN_GUID + "_stylebonusdefault_";
        private static ConfigDivision customStyleBonusesDivision;
        private static readonly HashSet<string> registeredStyleIDs = [];
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

        internal static void Initialize(Plugin plugin)
        {
            PluginConfigurator config = PluginConfigurator.Create(MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_GUID);
            config.icon = Plugin.Assets.LoadAsset<Sprite>("Icon");

            #region HUD
            hudAcceleration = new BoolField(config.rootPanel, "HUD FOLLOWS SPEED", "hudacceleration", true);

            #region Crosshair HUD
            new ConfigHeader(config.rootPanel, "CROSSHAIR HUD");

            alternateCrosshair = new BoolField(config.rootPanel, "ALTERNATE CROSSHAIR", "use_alt_crosshair", false);
            alternateCrosshair.postValueChangeEvent += _ => CanvasController.Instance?.crosshair.CheckCrossHair();

            crosshairRailcannonCharge = new BoolField(config.rootPanel, "RAILCANNON CHARGE", "crosshair_rc_charge", false);
            crosshairRailcannonCharge.postValueChangeEvent += _ => Patches.CrosshairRailcannonSlider?.SetActive(CrosshairRailcannonCharge);

            crosshairFistIcon = new BoolField(config.rootPanel, "FIST ICON", "crosshair_fist_icon", false);
            crosshairFistIcon.postValueChangeEvent += value => Patches.CrosshairFistIconFill?.parent.SetActive(value);

            crosshairWeaponIcon = new BoolField(config.rootPanel, "WEAPON ICON", "crosshair_weapon_icon", false);
            crosshairWeaponIcon.postValueChangeEvent += value => Patches.CrosshairWeaponIcon?.SetActive(value);
            #endregion

            #region Status HUD
            new ConfigHeader(config.rootPanel, "STATUS HUD");

            hardDamageNumber = new BoolField(config.rootPanel, "HARD DAMAGE INDICATOR", "show_hard_damage", false);
            hardDamageNumber.postValueChangeEvent += value => Patches.HardDamageNumber?.SetActive(value);

            altIndicatorPosition = new BoolField(config.rootPanel, "ALTERNATE WEAPON ICON POSITION", "alt_equipped_indicator_pos", false);
            altIndicatorPosition.postValueChangeEvent += value =>
            {
                forceAltRailcannonCharge.interactable = !value;
                Patches.SetIconParent(value);
            };

            forceAltRailcannonCharge = new BoolField(config.rootPanel, "FORCE ALTERNATE RAILCANNON DISPLAY", "force_alt_railcannon_charge", false) { interactable = !altIndicatorPosition.value };
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
                    if (StyleHUD.Instance is not { } styleHUD || index == styleHUD.rankIndex) { return; }
                    styleHUD.rankImage.sprite = spriteField.Sprite ?? styleHUD.ranks[index].sprite;
                }
            }

            new ConfigHeader(config.rootPanel, "CUSTOM STYLE BONUSES", 16);
            new ConfigHeader(config.rootPanel, "NEW STYLE BONUSES ARE AUTOMATICALLY REGISTERED WHEN ENCOUNTERED\nKEEP PLAYING TO FIND ADDITIONAL STYLE BONUSES", 12);
            customStyleBonusesDivision = new ConfigDivision(config.rootPanel, "customstylebonuses");
            plugin.StartCoroutine(AddStyleBonuses());
            static IEnumerator AddStyleBonuses()
            {
                yield return new WaitUntil(() => StyleHUD.Instance && PrefsManager.Instance);
                PrefsManager.Instance.localPrefMap.DoIf(pair => pair.Key.StartsWith(DefaultStyleGUID), pair =>
                {
                    AddStyleBonusEntry(pair.Key[DefaultStyleGUID.Length..]);
                });
            }
            #endregion

            #region Other
            new ConfigHeader(config.rootPanel, "OTHER");
            fupAlert = new BoolField(config.rootPanel, "ROCKET WHIPLASH ALERT", "fup_alert", false);
            #endregion
            #endregion

            #region Viewmodel
            new ConfigHeader(config.rootPanel, "VIEWMODEL");

            walkingBob = new BoolField(config.rootPanel, "WALKING BOB", "walkingbob", true);
            walkingBob.postValueChangeEvent += value =>
            {
                if (NewMovement.Instance?.GetComponentInChildren<WalkingBob>(true) is not { } bob) { return; }
                bob.enabled = value;
                bob.transform.localPosition = Vector3.zero;
            };

            viewmodelAcceleration = new BoolField(config.rootPanel, "WEAPONS FOLLOW SPEED", "viewmodelacceleration", true);
            #endregion
        }

        internal static void AddStyleBonusEntry(string id, string defaultValue = null)
        {
            if (!registeredStyleIDs.Add(id)) { return; }

            if (defaultValue == null)
            {
                defaultValue = PrefsManager.Instance.GetStringLocal(DefaultStyleGUID + id, id);
            }
            else
            {
                PrefsManager.Instance.SetStringLocal(DefaultStyleGUID + id, defaultValue);
            }
            
            StringField stringField = new(customStyleBonusesDivision, defaultValue.IsNullOrWhiteSpace() ? id : defaultValue, "customstylebonus_" + id, defaultValue, true);
            stringField.postValueChangeEvent += SetStyleBonusText;
            SetStyleBonusText(stringField.value);
            
            void SetStyleBonusText(string text) => StyleHUD.Instance.idNameDict.TrySetValue(id, text, true);
        }
    }
}
