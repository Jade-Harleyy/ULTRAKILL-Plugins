using UnityEngine;

namespace JadeLib
{
    public static class VectorExtensions
    {
        public static Vector2 SetX(this Vector2 v, float x) => new(x, v.y);
        public static Vector2 SetY(this Vector2 v, float y) => new(v.x, y);
        public static Vector2 Sqrt(Vector2 v) => v.normalized * Mathf.Sqrt(v.magnitude);

        public static Vector3 SetX(this Vector3 v, float x) => new(x, v.y, v.z);
        public static Vector3 SetY(this Vector3 v, float y) => new(v.x, y, v.z);
        public static Vector3 SetZ(this Vector3 v, float z) => new(v.x, v.y, z);
        public static Vector3 Sqrt(Vector3 v) => v.normalized * Mathf.Sqrt(v.magnitude);
    }
}
