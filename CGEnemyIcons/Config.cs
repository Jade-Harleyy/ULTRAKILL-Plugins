using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using JadeLib;

namespace CGEnemyIcons
{
    public enum OnDeath
    {
        Marker = 0,
        Remove = 1,
        RemoveIfAllDead = 2
    }

    public enum FilterType
    {
        Off = 0,
        RadiantOnly = 1,
        PreferRadiant = 2,
        Both = 3
    }

    public static class Config
    {
        public static EnumField<OnDeath> onDeath;
        public static List<(string name, EnemyCategory type, EnumField<FilterType> field)> showEnemies = [];
        public static Dictionary<(EnemyCategory self, EnemyCategory other), BoolField> hideCategories = [];

        public static void Init()
        {
            PluginConfigurator config = PluginConfigurator.Create(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);
            config.icon = Plugin.assets.LoadAsset<Sprite>("Icon");

            onDeath = new(config.rootPanel, "Action on Death", "onDeath", OnDeath.Marker);
            onDeath.SetEnumDisplayNames("Cross Out", "Remove", "Remove if all Dead");

            new ConfigHeader(config.rootPanel, "Enemy Types");
            PrefabDatabase prefabs = Addressables.LoadAssetAsync<PrefabDatabase>("Assets/Data/Cyber Grind Patterns/Data/Prefab Database.asset").WaitForCompletion();

            new ConfigHeader(config.rootPanel, "Common Enemies", 18);
            CreateHiders(EnemyCategory.Commons);
            foreach (EndlessEnemy enemy in prefabs.meleeEnemies)
            {
                string name = enemy.prefab.GetComponentInChildren<EnemyIdentifier>(true).FullName;
                showEnemies.Add((name, EnemyCategory.Commons, new(config.rootPanel, $"Show {name}", name, FilterType.Both)));
            }
            foreach (EndlessEnemy enemy in prefabs.projectileEnemies)
            {
                string name = enemy.prefab.GetComponentInChildren<EnemyIdentifier>(true).FullName;
                showEnemies.Add((name, EnemyCategory.Commons, new(config.rootPanel, $"Show {name}", name, FilterType.Both)));
            }

            new ConfigHeader(config.rootPanel, "Uncommon Enemies", 18);
            CreateHiders(EnemyCategory.Uncommons);
            foreach (EndlessEnemy enemy in prefabs.uncommonEnemies)
            {
                string name = enemy.prefab.GetComponentInChildren<EnemyIdentifier>(true).FullName;
                showEnemies.Add((name, EnemyCategory.Uncommons, new(config.rootPanel, $"Show {name}", name, FilterType.Both)));
            }

            new ConfigHeader(config.rootPanel, "Special Enemies", 18);
            CreateHiders(EnemyCategory.Specials);
            foreach (EndlessEnemy enemy in prefabs.specialEnemies)
            {
                string name = enemy.prefab.GetComponentInChildren<EnemyIdentifier>(true).FullName;
                showEnemies.Add((name, EnemyCategory.Specials, new(config.rootPanel, $"Show {name}", name, FilterType.Both)));
            }

            new ConfigHeader(config.rootPanel, "Hideous Masses", 18);
            CreateHiders(EnemyCategory.Masses);
            string massName = prefabs.hideousMass.GetComponentInChildren<EnemyIdentifier>(true).FullName;
            showEnemies.Add((massName, EnemyCategory.Masses, new(config.rootPanel, $"Show {massName}", massName, FilterType.Both)));
            foreach (EnumField<FilterType> field in showEnemies.Select(i => i.field))
            {
                field.SetEnumDisplayNames("No", "Radiant Only", "Prefer Radiant", "Yes");
            }

            void CreateHiders(EnemyCategory self) => self.DoIf(T => T != self, T => hideCategories.Add((self, T), new(config.rootPanel, $"Hide if {T.Name()} Alive", $"hide{self.Name()}If{T.Name()}", false)));
        }
    }
}
