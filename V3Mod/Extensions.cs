using System;
using UnityEngine;
using static UnityEngine.Object;

namespace V3Mod
{
    public static class Extensions
    {
        public static Vector3 SetY(this Vector3 v, float y) => new(v.x, y, v.z);

        public static Vector3 Sqrt(Vector3 v) => v.normalized * Mathf.Sqrt(v.magnitude);

        public static GameObject InstantiateInactive(GameObject original)
        {
            GameObject clone = Instantiate(original);
            clone.SetActive(false);
            return clone;
        }

        public static GameObject InstantiateInactive(GameObject original, Transform parent)
        {
            GameObject clone = Instantiate(original, parent);
            clone.SetActive(false);
            return clone;
        }

        public static void AddToArray<T>(ref T[] array, T obj, int index)
        {
            T[] newArray = new T[Math.Max(index, array.Length) + 1];
            newArray[index] = obj;
            Array.Copy(array, newArray, Math.Min(index, array.Length));
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            array = newArray;
        }

        public static string ToHexString(this Color clr) => $"#{Mathf.RoundToInt(clr.r * 255):X2}{Mathf.RoundToInt(clr.g * 255):X2}{Mathf.RoundToInt(clr.b * 255):X2}{Mathf.RoundToInt(clr.a * 255):X2}";
    }
}