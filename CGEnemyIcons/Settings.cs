using HarmonyLib;
using JadeLib;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CGEnemyIcons
{
    internal enum OnDeath
    {
        Marker,
        Remove,
        RemoveIfAllDead
    }

    internal enum FilterType
    {
        Both,
        RadiantOnly,
        PreferRadiant,
        Off
    }

    internal static class Settings
    {
        private static EnumField<OnDeath> onDeath;
        public static OnDeath OnDeath => onDeath.value;

        private static readonly Dictionary<EnumField<FilterType>, (string name, EnemyCategory type)> showEnemies = [];
        public static IEnumerable<(FilterType filter, string name, EnemyCategory type)> ShowEnemies => showEnemies.Select(kvp => (kvp.Key.value, kvp.Value.name, kvp.Value.type));

        private static readonly Dictionary<BoolField, (EnemyCategory self, EnemyCategory other)> hideCategories = [];
        public static IEnumerable<(EnemyCategory self, EnemyCategory other)> HideCategories => hideCategories.Where(kvp => kvp.Key.value).Select(kvp => kvp.Value);

        internal static void Initialize()
        {
            PluginConfigurator config = PluginConfigurator.Create(MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_GUID);
            config.icon = Plugin.Assets.LoadAsset<Sprite>("Icon");

            onDeath = new EnumField<OnDeath>(config.rootPanel, "ACTION ON DEATH", "onDeath", OnDeath.Marker);
            onDeath.SetEnumDisplayName(OnDeath.Marker, "CROSS OUT");
            onDeath.SetEnumDisplayName(OnDeath.Remove, "REMOVE");
            onDeath.SetEnumDisplayName(OnDeath.RemoveIfAllDead, "REMOVE IF ALL DEAD");

            new ConfigHeader(config.rootPanel, "ENEMY TYPES");
            PrefabDatabase prefabs = Addressables.LoadAssetAsync<PrefabDatabase>("Assets/Data/Cyber Grind Patterns/Data/Prefab Database.asset").WaitForCompletion();

            AddEnemies(prefabs.meleeEnemies.Concat(prefabs.projectileEnemies), EnemyCategory.Common);
            AddEnemies(prefabs.uncommonEnemies, EnemyCategory.Uncommon);
            AddEnemies(prefabs.specialEnemies, EnemyCategory.Special);

            CreateCategory(EnemyCategory.Mass, "HIDEOUS MASSES");
            AddEnemy(prefabs.hideousMass, EnemyCategory.Mass);

            void CreateCategory(EnemyCategory category, string categoryName)
            {
                new ConfigHeader(config.rootPanel, categoryName, 18);
                category.DoIf(T => T != category, T => hideCategories.Add(new BoolField(config.rootPanel, $"HIDE IF {T.Name().ToUpperInvariant()} ALIVE", $"hide{category.Name().ToLowerInvariant()}if{T.Name().ToLowerInvariant()}", false), (category, T)));
            }

            void AddEnemies(IEnumerable<EndlessEnemy> enemies, EnemyCategory category)
            {
                CreateCategory(category, category.Name().ToUpperInvariant() + " ENEMIES");
                enemies.Do(enemy => AddEnemy(enemy.prefab, category));
            }

            void AddEnemy(GameObject enemy, EnemyCategory category)
            {
                string name = enemy.GetComponentInChildren<EnemyIdentifier>(true).FullName;

                EnumField<FilterType> field = new(config.rootPanel, $"SHOW {name.ToUpperInvariant()}", name, FilterType.Both);
                field.SetEnumDisplayName(FilterType.Both, "YES");
                field.SetEnumDisplayName(FilterType.RadiantOnly, "RADIANT ONLY");
                field.SetEnumDisplayName(FilterType.PreferRadiant, "PREFER RADIANT");
                field.SetEnumDisplayName(FilterType.Off, "NO");

                showEnemies.Add(field, (name, category));
            }
        }
    }
}
