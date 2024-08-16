using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JadeLib
{
    public static class EnumerableExtensions
    {
        public static void AddToArray<T>(ref T[] array, int index, params T[] values)
        {
            List<T> list = [.. array];
            list.InsertRange(Mathf.Clamp(index, 0, list.Count), values);
            array = [.. list];
        }

        public static void AddToArray<T>(ref T[] array, int index, params T[][] values)
        {
            List<T> list = [.. array];
            list.InsertRange(Mathf.Clamp(index, 0, list.Count), values.Collapse());
            array = [.. list];
        }

        public static IEnumerable<T> Collapse<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(values => values);

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector) => source.GroupBy(selector).Select(group => group.First());

        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate = null) => predicate == null ? !source.Any() : !source.Any(predicate);

        public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            IEnumerable<T> result = source.SelectMany(selector);
            return !result.Any() ? result : result.Concat(result.SelectManyRecursive(selector));
        }

        public static IEnumerable<T> CollapseRecursive<T>(this IEnumerable<IEnumerable<T>> source)
        {
            IEnumerable<T> result = source.Collapse();
            return result is not IEnumerable<IEnumerable<T>> result2 ? result : result.Concat(result2.CollapseRecursive());
        }
    }
}
