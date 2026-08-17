using UnityEngine;

namespace Subject626
{
    /// <summary>Helpers para construir geometria con primitivas (cajas, paredes, cilindros).</summary>
    public static class Prim
    {
        /// <summary>Destruye sirviendo tanto en Play (Destroy) como al hornear en editor (DestroyImmediate).</summary>
        public static void DestroySafe(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }

        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, bool collider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) DestroySafe(c);
            }
            return go;
        }

        public static GameObject BoxRot(Transform parent, string name, Vector3 center, Vector3 size, Quaternion rot, Material mat, bool collider = true)
        {
            GameObject go = Box(parent, name, center, size, mat, collider);
            go.transform.localRotation = rot;
            return go;
        }

        public static GameObject Cyl(Transform parent, string name, Vector3 center, float radius, float height, Material mat, bool collider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) DestroySafe(c);
            }
            return go;
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 center, float diameter, Material mat, bool collider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = new Vector3(diameter, diameter, diameter);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null) DestroySafe(c);
            }
            return go;
        }

        public static Light PointLight(Transform parent, Vector3 pos, Color color, float range, float intensity)
        {
            GameObject go = new GameObject("Light");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
            return l;
        }
    }
}
