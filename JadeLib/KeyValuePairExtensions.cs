﻿using System.Collections.Generic;

namespace JadeLib
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value) => (key, value) = (kvp.Key, kvp.Value);
    }
}
