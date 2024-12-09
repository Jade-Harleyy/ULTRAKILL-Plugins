using System;

namespace JadeLib
{
    public static class EnumExtensions
    {
        public static void DoIf<T>(this T _, Func<T, bool> condition, Action<T> action) where T : Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (condition.Invoke(value)) { action.Invoke(value); }
            }
        }

        public static string Name<T>(this T value) where T : Enum => Enum.GetName(typeof(T), value);
    }
}
