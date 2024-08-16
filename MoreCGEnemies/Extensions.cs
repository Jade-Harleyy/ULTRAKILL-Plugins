using System;

namespace MoreCGEnemies
{
    public static class Extensions
    {
        public static void AddToArray<T>(ref T[] array, int index, params T[] objs)
        {
            T[] newArray = new T[Math.Max(index, array.Length) + objs.Length];
            Array.Copy(array, newArray, Math.Min(index, array.Length));
            Array.Copy(objs, 0, newArray, index, objs.Length);
            Array.Copy(array, index, newArray, index + objs.Length, array.Length - index);
            array = newArray;
        }
    }
}
