using System;
using JadeLib;
using PluginConfig.API;
using PluginConfig.API.Fields;
using UnityEngine;

namespace DamageNumbers
{
    internal static class Settings
    {
        private static ColorField color;
        private static FloatSliderField opacity;
        public static Color Color => color.value with { a = opacity.value };
        
        private static BoolField alwaysOnTop;
        public static bool AlwaysOnTop => alwaysOnTop.value;
        
        #region Size
        private static BoolField scaleNumbers;
        public static bool ScaleNumbers => scaleNumbers.value;
        
        private static FloatField sizeMultiplier;
        public static float SizeMultiplier => sizeMultiplier.value;
        
        private static FloatField size;
        public static float Size => size.value;
        #endregion
        
        #region Removal Time
        private static BoolField scaleTime;
        public static bool ScaleTime => scaleTime.value;
        
        private static FloatField timeMultiplier;
        public static float TimeMultiplier => timeMultiplier.value;
        
        private static FloatField time;
        public static float Time => time.value;
        #endregion
        
        private static FloatField offsetMultiplier;
        public static float OffsetMultiplier => offsetMultiplier.value;

        internal static void Initialize()
        {
            PluginConfigurator config = PluginConfigurator.Create(MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_GUID);
            config.icon = ImageImporter.LoadFromFile($"{Plugin.ModDir}/icon.png", out _);

            color = new ColorField(config.rootPanel, "COLOR", "color", Color.red);
            opacity = new FloatSliderField(config.rootPanel, "OPACITY", "alpha", new Tuple<float, float>(0f, 1f), 0.75f, 2);
            alwaysOnTop = new BoolField(config.rootPanel, "ALWAYS ON TOP", "alwaysontop", false);

            #region Size
            scaleNumbers = new BoolField(config.rootPanel, "SCALE SIZE TO DAMAGE", "scalesize", true);
            sizeMultiplier = new FloatField(config.rootPanel, "SIZE MULTIPLIER", "sizemult", 1f, 0f, float.MaxValue) { hidden = !scaleNumbers.value };
            size = new FloatField(config.rootPanel, "SIZE", "size", 12f, 0f, float.MaxValue) { hidden = scaleNumbers.value };
            scaleNumbers.postValueChangeEvent += value =>
            {
                sizeMultiplier.hidden = !value;
                size.hidden = value;
            };
            #endregion

            #region Removal Time
            scaleTime = new BoolField(config.rootPanel, "SCALE REMOVAL TIME TO DAMAGE", "scaletime", true);
            timeMultiplier = new FloatField(config.rootPanel, "REMOVAL TIME MULTIPLIER", "timemult", 1f, 0f, float.MaxValue) { hidden = !scaleTime.value };
            time = new FloatField(config.rootPanel, "REMOVAL TIME", "time", 2.5f, 0f, float.MaxValue) { hidden = scaleTime.value };
            scaleTime.postValueChangeEvent += value =>
            {
                timeMultiplier.hidden = !value;
                time.hidden = value;
            };
            #endregion

            offsetMultiplier = new FloatField(config.rootPanel, "RANDOM OFFSET", "maxoffset", 2.5f, 0f, float.MaxValue);
        }
    }
}