using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Subject626.EditorTools
{
    /// <summary>Crea/hornea la escena de juego y buildea.</summary>
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Subject626.unity";

        [MenuItem("Subject626/Crear escena (procedural, se arma al Play)")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();

            Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Debug.Log("[Subject626] Escena procedural creada en " + ScenePath);
        }

        [MenuItem("Subject626/Hornear sala en la escena (editable a mano)")]
        public static void BakeRoom()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();

            // Genera la sala + backstage como GameObjects reales en la escena.
            MaterialLib.ClearCache();
            RoomBuilder.Build();

            // Persistir materiales generados por codigo como assets (si no, salen "rosa" al recargar).
            SaveGeneratedAssets();

            Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Debug.Log("[Subject626] Sala HORNEADA en " + ScenePath + ". Ahora la podes editar a mano; el Play no la regenera.");
        }

        static void SaveGeneratedAssets()
        {
            Directory.CreateDirectory("Assets/Generated/Materials");
            Directory.CreateDirectory("Assets/Generated/Physics");

            var seenMat = new HashSet<Material>();
            foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = mats[i];
                    if (m == null || EditorUtility.IsPersistent(m)) continue;
                    if (seenMat.Add(m))
                    {
                        string p = AssetDatabase.GenerateUniqueAssetPath("Assets/Generated/Materials/" + Sanitize(m.name) + ".mat");
                        AssetDatabase.CreateAsset(m, p);
                    }
                }
            }

            // Materiales de fisica (no criticos): si algo falla, seguimos igual.
            try
            {
                var seenPhys = new HashSet<PhysicsMaterial>();
                foreach (Collider c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    PhysicsMaterial pm = c.sharedMaterial;
                    if (pm == null || EditorUtility.IsPersistent(pm)) continue;
                    if (seenPhys.Add(pm))
                    {
                        string p = AssetDatabase.GenerateUniqueAssetPath("Assets/Generated/Physics/" + Sanitize(pm.name) + ".physicMaterial");
                        AssetDatabase.CreateAsset(pm, p);
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Subject626] No se pudo persistir material de fisica: " + e.Message); }

            AssetDatabase.SaveAssets();
        }

        static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "M";
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i])) chars[i] = '_';
            return new string(chars);
        }

        static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in list) if (s.path == path) found = true;
            if (!found) list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        [MenuItem("Subject626/Build Windows")]
        public static void BuildWindows()
        {
            CreateScene();
            var opts = new BuildPlayerOptions();
            opts.scenes = new[] { ScenePath };
            opts.locationPathName = "Build/Subject626.exe";
            opts.target = BuildTarget.StandaloneWindows64;
            opts.options = BuildOptions.None;
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("[Subject626] Build result: " + report.summary.result + "  errors: " + report.summary.totalErrors);
        }
    }
}
