using HarmonyLib;
using UnityEngine;

namespace CGModifiers
{
    [ConfigureSingleton(SingletonFlags.NoAutoInstance)]
    public class ModifierManager : MonoSingleton<ModifierManager>
    {
        private WaveMenu wm;

        public FloatSelector waveSelect;

        [Header("Wave Modifiers")]
        public FloatSelector startingPointsSelector;
        public FloatSelector pointMultiplierSelector;

        [Header("Enemy Modifiers")]
        //public StringSelector forceRadianceSelector; //TODO
        public BoolSelector forceRadianceSelector;
        public FloatSelector initialUncommonsSelector; //TODO
        public FloatSelector uncommonsIncreaseSelector; //TODO
        public FloatSelector initialSpecialsSelector; //TODO
        public FloatSelector specialsIncreaseSelector; //TODO

        [Header("Arena Modifiers")]
        public BoolSelector scoreboardToggler;
        public FloatSelector zapZoneHeightSelector;
        public BoolSelector visibleOOBSelector;
        public Material OOBMaterial;

        [Header("Player Modifiers")]
        public FloatSelector maxStaminaSelector; //TODO
        public FloatSelector staminaRegenSelector; //TODO

        private void Start()
        {
            GetComponentsInChildren<FloatSelector>(true).Do(s => s.Init());
            GetComponentsInChildren<BoolSelector>(true).Do(s => s.Init());
            GetComponentsInChildren<StringSelector>(true).Do(s => s.Init());
            waveSelect.Init();

            wm = waveSelect.GetComponentInParent<WaveMenu>();

            waveSelect.maxValue = Mathf.Min(Traverse.Create(wm).Field("highestWave").GetValue<int>() / 2, 25);
            startingPointsSelector.minValue = Traverse.Create(EndlessGrid.Instance).Field("maxPoints").GetValue<int>();

            waveSelect.onValueChanged += (value) => wm.SetCurrentWave((int)value);
        }
    }
}