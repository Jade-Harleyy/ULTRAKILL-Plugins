using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using JadeLib;

namespace CGEnemyIcons
{
    public enum EnemyCategory
    {
        Commons = 0,
        Uncommons = 1,
        Specials = 2,
        Masses = 3
    }

    [HarmonyPatch]
    internal static class Patches
    {
        public static IEnumerable<SpawnableObject> enemies;
        public static GameObject iconsCanvas, iconPrefab, levelStats;
        public static Dictionary<EnemyIdentifier, (EnemyCategory type, bool radiant, GameObject obj)> icons = [];

        [HarmonyPatch(typeof(EndlessGrid), "Start"), HarmonyPostfix]
        private static void EndlessGrid_Start()
        {
            enemies = Resources.FindObjectsOfTypeAll<SpawnableObjectsDatabase>().SelectMany(db => db.enemies);

            iconsCanvas = Object.Instantiate(Plugin.assets.LoadAsset<GameObject>("Enemy Icons"), CanvasController.Instance.transform.Find("Level Stats Controller"));
            levelStats = CanvasController.Instance.transform.Find("Level Stats Controller/Level Stats (1)").gameObject;

            iconPrefab = Plugin.assets.LoadAsset<GameObject>("Enemy Icon");
        }

        [HarmonyPatch(typeof(EndlessGrid), "SpawnOnGrid"), HarmonyPostfix]
        private static void EndlessGrid_SpawnOnGrid(GameObject obj, bool radiant, ref GameObject __result, PrefabDatabase ___prefabs)
        {
            if (__result?.GetComponentInChildren<EnemyIdentifier>(true) is not EnemyIdentifier eid) return;

            GameObject icon = Object.Instantiate(iconPrefab, Vector2.zero, Quaternion.identity, iconsCanvas.transform);
            icon.transform.Find("Icon").GetComponent<Image>().sprite = enemies.FirstOrDefault(spawnable => spawnable.gameObject.GetComponentInChildren<EnemyIdentifier>(true)?.FullName == eid.FullName)?.gridIcon;
            if (radiant) icon.transform.Find("Radiant").gameObject.SetActive(true);
            icon.name = eid.FullName;

            EnemyCategory type = EnemyCategory.Commons;
            if (___prefabs.uncommonEnemies.Any(prefab => prefab.prefab == obj))
            {
                type = EnemyCategory.Uncommons;
            }
            else if (___prefabs.specialEnemies.Any(prefab => prefab.prefab == obj))
            {
                type = EnemyCategory.Specials;
            }
            else if (obj == ___prefabs.hideousMass)
            {
                type = EnemyCategory.Masses;
            }
            icons.Add(eid, (type, radiant, icon));

            foreach (GameObject obj2 in icons.OrderBy(kvp => (kvp.Value.type, EnemyTracker.Instance.GetEnemyRank(kvp.Key), kvp.Key.FullName, kvp.Key.GetInstanceID())).Select(kvp => kvp.Value.obj))
            {
                obj2.transform.SetAsFirstSibling();
            }
        }

        [HarmonyPatch(typeof(EndlessGrid), "Update"), HarmonyPostfix]
        private static void EndlessGrid_Update()
        {
            (iconsCanvas.transform as RectTransform).anchoredPosition = levelStats.activeSelf ? new(0, -220) : Vector2.zero;

            foreach ((EnemyIdentifier eid, (EnemyCategory type, bool radiant, GameObject icon)) in icons)
            {
                if (Config.hideCategories.Any(hiders => hiders.Key.self == type && hiders.Value.value && icons.Any(icon => !icon.Key.dead && icon.Value.type == hiders.Key.other)))
                {
                    icon.SetActive(false);
                    continue;
                }

                icon.transform.Find("Dead").gameObject.SetActive(eid.dead);
                icon.transform.Find("Idoled").gameObject.SetActive(eid.blessed);
                switch (Config.showEnemies.FirstOrDefault(item => item.name == eid.FullName && item.type == type).field?.value, Config.onDeath.value)
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
                        icon.SetActive(radiant && AnyRadiantAlive() || !radiant && !AnyRadiantAlive() && AnyAlive());
                        break;
                    case (null or FilterType.Both, OnDeath.Remove):
                        icon.SetActive(!eid.dead);
                        break;
                    case (null or FilterType.Both, OnDeath.Marker):
                        icon.SetActive(true);
                        break;
                    case (null or FilterType.Both, OnDeath.RemoveIfAllDead):
                        icon.SetActive(AnyAlive());
                        break;
                }

                bool AnyAlive() => icons.Any(icon => icon.Key.FullName == eid.FullName && !icon.Key.dead);
                bool AnyRadiantAlive() => icons.Any(icon => icon.Key.FullName == eid.FullName && icon.Value.radiant && !icon.Key.dead);
            }

            iconsCanvas.SetActive(icons.Any(kvp => kvp.Value.obj.activeSelf));
        }

        [HarmonyPatch(typeof(EndlessGrid), "NextWave"), HarmonyPostfix]
        private static void EndlessGrid_NextWave()
        {
            icons.Do(icon => Object.Destroy(icon.Value.obj));
            icons.Clear();
        }
    }
}
