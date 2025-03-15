using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable once UnusedType.Global
namespace System.Runtime.CompilerServices { public class IsExternalInit; }

namespace CGEnemyIcons
{
    internal enum EnemyCategory { Common, Uncommon, Special, Mass }

    internal readonly record struct IconInfo(EnemyIdentifier Enemy, EnemyCategory Type, bool IsRadiant, GameObject IconObject) : IComparable<IconInfo>
    {
        // ReSharper disable once UnusedMember.Global
        public bool Equals(IconInfo? other) => other.HasValue && other.Value.Enemy == Enemy;
        public override int GetHashCode() => Enemy.GetInstanceID();

        public int CompareTo(IconInfo other)
        {
            int typeComparison = Type.CompareTo(other.Type);
            if (typeComparison != 0) { return typeComparison; }

            int rankComparison = EnemyTracker.Instance.GetEnemyRank(Enemy).CompareTo(EnemyTracker.Instance.GetEnemyRank(other.Enemy));
            if (rankComparison != 0) { return rankComparison; }

            int nameComparison = string.Compare(Enemy.FullName, other.Enemy.FullName, StringComparison.InvariantCultureIgnoreCase);
            return nameComparison != 0 ? nameComparison : Enemy.GetInstanceID().CompareTo(other.Enemy.GetInstanceID());
        }
    }

    [HarmonyPatch]
    internal static class Patches
    {
        private static readonly IEnumerable<SpawnableObject> enemies = Resources.FindObjectsOfTypeAll<SpawnableObjectsDatabase>().SelectMany(db => db.enemies);
        private static readonly GameObject iconPrefab = Plugin.Assets.LoadAsset<GameObject>("Enemy Icon");
        private static GameObject iconsCanvas, levelStats;
        private static readonly SortedSet<IconInfo> icons = [];

        [HarmonyPatch(typeof(EndlessGrid), "Start"), HarmonyPostfix]
        private static void EndlessGrid_Start()
        {
            iconsCanvas = Object.Instantiate(Plugin.Assets.LoadAsset<GameObject>("Enemy Icons"),  CanvasController.Instance?.transform.Find("Level Stats Controller"));
            levelStats = CanvasController.Instance?.transform.Find("Level Stats Controller/Level Stats (1)").gameObject;
            icons.Clear();
        }

        [HarmonyPatch(typeof(EndlessGrid), "SpawnOnGrid"), HarmonyPostfix]
        private static void EndlessGrid_SpawnOnGrid(GameObject obj, bool radiant, GameObject __result, PrefabDatabase ___prefabs)
        {
            if (__result?.GetComponentInChildren<EnemyIdentifier>(true) is not { } eid) { return; }

            GameObject icon = Object.Instantiate(iconPrefab, Vector2.zero, Quaternion.identity, iconsCanvas.transform);
            icon.transform.Find("Icon").GetComponent<Image>().sprite = enemies.FirstOrDefault(spawnable => spawnable.gameObject.GetComponentInChildren<EnemyIdentifier>(true)?.FullName == eid.FullName)?.gridIcon;
            if (radiant) { icon.transform.Find("Radiant").gameObject.SetActive(true); }
            icon.name = eid.FullName;
            
            EnemyCategory type = EnemyCategory.Common;
            if (___prefabs.uncommonEnemies.Any(prefab => prefab.prefab == obj))
            {
                type = EnemyCategory.Uncommon;
            }
            else if (___prefabs.specialEnemies.Any(prefab => prefab.prefab == obj))
            {
                type = EnemyCategory.Special;
            }
            else if (obj == ___prefabs.hideousMass)
            {
                type = EnemyCategory.Mass;
            }

            icons.Add(new IconInfo(eid, type, radiant, icon));
            icons.Do(icon2 => icon2.IconObject.transform.SetAsLastSibling());
        }

        [HarmonyPatch(typeof(EndlessGrid), "Update"), HarmonyPostfix]
        private static void EndlessGrid_Update()
        {
            ((RectTransform)iconsCanvas.transform).anchoredPosition = levelStats.activeSelf ? new Vector2(0, -220) : Vector2.zero;

            foreach ((EnemyIdentifier eid, EnemyCategory type, bool radiant, GameObject icon) in icons)
            {
                if (!eid || Settings.HideCategories.Any(hiders => hiders.self == type && icons.Any(otherIcon => otherIcon.Type == hiders.other && !otherIcon.Enemy.dead)))
                {
                    icon.SetActive(false);
                    continue;
                }

                icon.transform.Find("Dead").gameObject.SetActive(eid.dead);
                icon.transform.Find("Idoled").gameObject.SetActive(eid.blessed);
                switch (Settings.ShowEnemies.FirstOrDefault(item => item.name == eid.FullName && item.type == type).filter, Settings.OnDeath)
                {
                    case (FilterType.Off, _):
                        icon.SetActive(false);
                        break;
                    case (FilterType.RadiantOnly, OnDeath.Remove):
                        icon.SetActive(radiant && !eid.dead);
                        break;
                    case (FilterType.RadiantOnly, OnDeath.Marker):
                        icon.SetActive(radiant);
                        break;
                    case (FilterType.RadiantOnly, OnDeath.RemoveIfAllDead):
                        icon.SetActive(radiant && AnyRadiantAlive());
                        break;
                    case (FilterType.PreferRadiant, OnDeath.Remove):
                        icon.SetActive((radiant || !AnyRadiantAlive()) && !eid.dead);
                        break;
                    case (FilterType.PreferRadiant, OnDeath.Marker):
                        icon.SetActive(radiant || !AnyRadiantAlive());
                        break;
                    case (FilterType.PreferRadiant, OnDeath.RemoveIfAllDead):
                        icon.SetActive(radiant ? AnyRadiantAlive() : !AnyRadiantAlive() && AnyAlive());
                        break;
                    case (FilterType.Both, OnDeath.Remove):
                        icon.SetActive(!eid.dead);
                        break;
                    case (FilterType.Both, OnDeath.Marker):
                        icon.SetActive(true);
                        break;
                    case (FilterType.Both, OnDeath.RemoveIfAllDead):
                        icon.SetActive(AnyAlive());
                        break;
                }

                bool AnyAlive() => icons.Any(otherIcon => otherIcon.Enemy.FullName == eid.FullName && !otherIcon.Enemy.dead);
                bool AnyRadiantAlive() => icons.Any(otherIcon => otherIcon.Enemy.FullName == eid.FullName && otherIcon.IsRadiant && !otherIcon.Enemy.dead);
            }

            iconsCanvas.SetActive(icons.Any(icon => icon.IconObject.activeSelf));
        }

        [HarmonyPatch(typeof(EndlessGrid), "NextWave"), HarmonyPostfix]
        private static void EndlessGrid_NextWave()
        {
            icons.Do(icon => Object.Destroy(icon.IconObject));
            icons.Clear();
        }
    }
}
