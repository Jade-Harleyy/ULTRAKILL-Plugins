using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using UnityEngine;

namespace BetterWeaponHUDs
{
    public static class Settings
    {
        private static BoolField useAlternateCrosshair;
        public static bool UseAlternateCrosshair => useAlternateCrosshair.value;

        private static BoolField showRailcannonCharge;
        public static bool ShowRailcannonCharge => showRailcannonCharge.value && RailcannonMeter.Instance.RailcannonStatus();

        private static BoolField showHardDamageNumber;
        public static bool ShowHardDamageNumber => showHardDamageNumber.value;

        private static BoolField altIndicatorPosition;
        public static bool AltIndicatorPosition => altIndicatorPosition.value;

        private static BoolField forceAltRailcannonCharge;
        public static bool ForceAltRailcannonCharge => forceAltRailcannonCharge.value;

        private static BoolField fupAlert;
        public static bool FUPAlert => fupAlert.value;

        internal static void Initialize()
        {
            PluginConfigurator config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.icon = Plugin.Assets.LoadAsset<Sprite>("Icon");

            new ConfigHeader(config.rootPanel, "CROSSHAIR HUD");
            useAlternateCrosshair = new(config.rootPanel, "ALTERNATE CROSSHAIR", "use_alt_crosshair", false);
            useAlternateCrosshair.postValueChangeEvent += _ => HUDOptions.Instance.crosshair.CheckCrossHair();
            showRailcannonCharge = new(config.rootPanel, "RAILCANNON CHARGE", "crosshair_rc_charge", false);

            new ConfigHeader(config.rootPanel, "STATUS HUD");
            showHardDamageNumber = new(config.rootPanel, "HARD DAMAGE INDICATOR", "show_hard_damage", false);
            showHardDamageNumber.postValueChangeEvent += _ => HudController.Instance.CheckSituation();
            altIndicatorPosition = new(config.rootPanel, "ALTERNATE WEAPON ICON POSITION", "alt_equipped_indicator_pos", false);
            altIndicatorPosition.postValueChangeEvent += value =>
            {
                forceAltRailcannonCharge.interactable = !value;
                RailcannonMeter.Instance.CheckStatus();
                HudController.Instance.CheckSituation();
            };
            forceAltRailcannonCharge = new(config.rootPanel, "FORCE ALTERNATE RAILCANNON DISPLAY", "force_alt_railcannon_charge", false) { interactable = !altIndicatorPosition.value };
            forceAltRailcannonCharge.postValueChangeEvent += _ => RailcannonMeter.Instance.CheckStatus();

            new ConfigHeader(config.rootPanel, "OTHER");
            fupAlert = new(config.rootPanel, "ROCKET WHIPLASH ALERT", "fup_alert", false);
        }
    }
}
