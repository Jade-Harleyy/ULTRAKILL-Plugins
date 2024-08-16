using HarmonyLib;
using UnityEngine;
using static MoreCGEnemies.Extensions;
using static UnityEngine.AddressableAssets.Addressables;

namespace MoreCGEnemies
{
    [HarmonyPatch(typeof(EndlessGrid))]
    public static class EndlessGridPatch
    {
        public static bool injected;

        [HarmonyPatch("Start"), HarmonyPrefix]
        public static void Start(PrefabDatabase ___prefabs)
        {
            if (injected) return;

            EndlessEnemy rocket = ScriptableObject.CreateInstance<EndlessEnemy>();
            rocket.name = "CentaurRocketLauncherEndlessData";
            rocket.enemyType = EnemyType.Centaur;
            rocket.prefab = LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/CentaurRocketLauncherStand.prefab").WaitForCompletion();
            rocket.prefab.transform.position = Vector2.zero;
            rocket.spawnCost = 30;
            rocket.spawnWave = 20;
            rocket.costIncreasePerSpawn = 15;

            EndlessEnemy mortar = ScriptableObject.CreateInstance<EndlessEnemy>();
            mortar.name = "CentaurMortarEndlessData";
            mortar.enemyType = EnemyType.Centaur;
            mortar.prefab = LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/CentaurMortar.prefab").WaitForCompletion();
            mortar.prefab.transform.position = Vector2.zero;
            mortar.spawnCost = 20;
            mortar.spawnWave = 15;
            mortar.costIncreasePerSpawn = 10;

            EndlessEnemy tower = ScriptableObject.CreateInstance<EndlessEnemy>();
            tower.name = "CentaurTowerEndlessData";
            tower.enemyType = EnemyType.Centaur;
            tower.prefab = LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/CentaurTower.prefab").WaitForCompletion();
            tower.prefab.transform.position = Vector2.zero;
            tower.spawnCost = 25;
            tower.spawnWave = 15;
            tower.costIncreasePerSpawn = 20;

            AddToArray(ref ___prefabs.projectileEnemies, ___prefabs.projectileEnemies.Length, rocket, mortar, tower);
            injected = true;
        }
    }
}
