using PluginConfig.API.Fields;
using System;

namespace JadeLib
{
    public static class PluginConfiguratorExtensions
    {
        public static void SetEnumDisplayNames<T>(this EnumField<T> field, params string[] names) where T : struct, Enum
        {
            T[] values = Enum.GetValues(typeof(T)) as T[];
            for (int i = 0; i < Math.Min(values.Length, names.Length); i++)
            {
                field.SetEnumDisplayName(values[i], names[i]);
            }
        }
    }
}
