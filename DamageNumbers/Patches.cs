using System;
using System.Globalization;
using HarmonyLib;
using JadeLib;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DamageNumbers
{
    [HarmonyPatch]
    internal static class Patches
    {
        private static readonly GameObject damageNumberObj = Plugin.Assets.LoadAsset<GameObject>("Damage Number");
        
        [HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.DeliverDamage)), HarmonyPrefix]
        private static void EnemyIdentifier_DeliverDamage_Pre(float ___health, out float __state) => __state = ___health;

        [HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.DeliverDamage)), HarmonyPostfix]
        private static void EnemyIdentifier_DeliverDamage_Post(Vector3 hitPoint, float __state, float ___health, float ___totalHealthModifier)
            => SpawnDamageNumber(hitPoint, GetDamageFromHP(__state, ___health, ___totalHealthModifier));

        private static float GetDamageFromHP(float prevHP, float currHP, float healthModifier)
        {
            float damageDealt = prevHP - MathF.Max(currHP, 0f);
            return MathF.Round(damageDealt * healthModifier, digits: 2);
        }
        
        private static void SpawnDamageNumber(Vector3 spawnPos, float damageDealt)
        {
            if (damageDealt < 0.01) { return; }
            
            GameObject obj = damageNumberObj.Instantiate(spawnPos);
            obj.transform.position += Random.insideUnitSphere * Settings.OffsetMultiplier;
            if (Settings.AlwaysOnTop) { obj.layer = LayerMask.NameToLayer("AlwaysOnTop"); }

            TMP_Text text = obj.GetComponent<TMP_Text>();
            text.SetText(damageDealt.ToString(CultureInfo.CurrentCulture));
            text.fontSize = Settings.ScaleNumbers
                ? Math.Clamp(damageDealt * 6f, 3f, 200f) * Settings.SizeMultiplier
                : Settings.Size;
            text.color = Settings.Color;

            RemoveOnTime removeOnTime = obj.GetComponent<RemoveOnTime>();
            removeOnTime.time = Settings.ScaleTime
                ? Math.Clamp(MathF.Log(damageDealt + 1f, 2f), 0.5f, 5f) * Settings.TimeMultiplier
                : Settings.Time;
        }
    }
}