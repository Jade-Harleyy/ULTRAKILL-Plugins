using System.Collections.Generic;

namespace JadeLib
{
    public static class DictionaryExtensions
    {
        public static void TrySetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, bool addIfNotContained = false)
        {
            if (!dictionary.ContainsKey(key))
            {
                if (addIfNotContained) { dictionary.Add(key, value); }
                return;
            }
            dictionary[key] = value;
        }
    }
}