using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Subject626
{
    /// <summary>
    /// Materiales URP creados por codigo y cacheados. Sin assets importados.
    /// La paleta "decorada" es para la habitacion; la gris plana es para el reveal greybox.
    /// </summary>
    public static class MaterialLib
    {
        static readonly Dictionary<int, Material> cache = new Dictionary<int, Material>();
        static Shader litShader;

        /// <summary>Vacia el cache (usado al hornear la sala en el editor, para no reusar materiales viejos).</summary>
        public static void ClearCache() { cache.Clear(); litShader = null; }

        static Shader Lit
        {
            get
            {
                if (litShader != null) return litShader;
                RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
                if (rp != null)
                {
                    if (rp.defaultMaterial != null) litShader = rp.defaultMaterial.shader;
                    if (litShader == null && rp.defaultShader != null) litShader = rp.defaultShader;
                }
                if (litShader == null) litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader == null) litShader = Shader.Find("Standard");
                if (litShader == null) litShader = Shader.Find("Sprites/Default");
                return litShader;
            }
        }

        static int Key(Color c, float smooth, float metal, bool emissive)
        {
            int h = 17;
            h = h * 31 + Mathf.RoundToInt(c.r * 255);
            h = h * 31 + Mathf.RoundToInt(c.g * 255);
            h = h * 31 + Mathf.RoundToInt(c.b * 255);
            h = h * 31 + Mathf.RoundToInt(c.a * 255);
            h = h * 31 + Mathf.RoundToInt(smooth * 100);
            h = h * 31 + Mathf.RoundToInt(metal * 100);
            h = h * 31 + (emissive ? 1 : 0);
            return h;
        }

        /// <summary>Material opaco liso con color, rugosidad y metalicidad.</summary>
        public static Material Solid(Color c, float smoothness = 0.15f, float metallic = 0f)
        {
            int k = Key(c, smoothness, metallic, false);
            Material m;
            if (cache.TryGetValue(k, out m) && m != null) return m;
            m = new Material(Lit);
            m.name = "M_" + ColorUtility.ToHtmlStringRGB(c);
            SetColor(m, c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            cache[k] = m;
            return m;
        }

        /// <summary>Material emisivo (para el cartel del reveal, pantallas, luces).</summary>
        public static Material Emissive(Color c, float intensity = 2f)
        {
            int k = Key(c, 0, 0, true) * 31 + Mathf.RoundToInt(intensity * 10);
            Material m;
            if (cache.TryGetValue(k, out m) && m != null) return m;
            m = new Material(Lit);
            m.name = "M_Emis_" + ColorUtility.ToHtmlStringRGB(c);
            SetColor(m, c);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * intensity);
            cache[k] = m;
            return m;
        }

        static void SetColor(Material m, Color c)
        {
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        }

        // Paleta "decorada" (la habitacion se ve terminada) ------------------
        public static Color WallPaper = new Color(0.62f, 0.55f, 0.44f);
        public static Color WoodFloor = new Color(0.42f, 0.29f, 0.17f);
        public static Color Ceiling = new Color(0.86f, 0.85f, 0.82f);
        public static Color Crate = new Color(0.58f, 0.42f, 0.23f);
        public static Color CrateDark = new Color(0.40f, 0.28f, 0.15f);
        public static Color Metal = new Color(0.55f, 0.57f, 0.60f);
        public static Color Red = new Color(0.62f, 0.16f, 0.14f);
        public static Color Green = new Color(0.22f, 0.45f, 0.24f);
        public static Color Blue = new Color(0.20f, 0.34f, 0.55f);
        public static Color Gold = new Color(0.85f, 0.68f, 0.24f);
        public static Color DoorWood = new Color(0.35f, 0.22f, 0.13f);
        public static Color Poster = new Color(0.80f, 0.30f, 0.25f);

        // Paleta "greybox" (afuera todo es prototipo sin arte) ---------------
        public static Color Grey = new Color(0.52f, 0.52f, 0.54f);
        public static Color GreyDark = new Color(0.34f, 0.34f, 0.36f);
        public static Color GreyLight = new Color(0.66f, 0.66f, 0.68f);
        public static Color DevOrange = new Color(0.95f, 0.45f, 0.10f);
    }
}
