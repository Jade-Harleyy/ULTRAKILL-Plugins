using UnityEngine;

namespace JadeLib
{
    public static class ObjectExtensions
    {
        public static T AddComponent<T>(this Component component) where T : Component => component.gameObject.AddComponent<T>();

        public static Transform Find(this Component component, string n) => component.transform.Find(n);
        public static Transform Find(this GameObject gameObject, string n) => gameObject.transform.Find(n);

        public static RectTransform FindAsRect(this Component component, string n) => component.transform.Find(n) as RectTransform;

        public static void SetActive(this Transform transform, bool value) => transform.gameObject.SetActive(value);

        public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays) => gameObject.transform.SetParent(parent.transform, worldPositionStays);

        public static T Instantiate<T>(this T original, Transform parent, bool worldPositionStays) where T : Object => Object.Instantiate(original, parent, worldPositionStays);
        public static T Instantiate<T>(this T original, Vector3 worldPos) where T : Object => Object.Instantiate(original, worldPos, Quaternion.identity);
    }
}
