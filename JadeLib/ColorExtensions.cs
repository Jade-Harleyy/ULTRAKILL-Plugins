using UnityEngine;

namespace JadeLib
{
    public static class ColorExtensions
    {
        public static string ToHexString(this Color clr) => $"#{Mathf.RoundToInt(clr.r * 255):X2}{Mathf.RoundToInt(clr.g * 255):X2}{Mathf.RoundToInt(clr.b * 255):X2}{Mathf.RoundToInt(clr.a * 255):X2}";
    }
}
