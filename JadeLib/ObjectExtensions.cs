using UnityEngine;

namespace JadeLib
{
    public static class ObjectExtensions
    {
        public static T AddComponent<T>(this Component component) where T : Component => component.gameObject.AddComponent<T>();

        public static Transform Find(this Component component, string n) => component.transform.Find(n);
        public static Transform Find(this GameObject gameObject, string n) => gameObject.transform.Find(n);

        public static void SetActive(this Transform transform, bool value) => transform.gameObject.SetActive(value);

        public static void SetParent(this GameObject gameObject, Transform parent, bool worldPositionStays) => gameObject.transform.SetParent(parent, worldPositionStays);
    }
}
