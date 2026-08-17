using UnityEditor;
using UnityEngine;

namespace Subject626.EditorTools
{
    /// <summary>
    /// Buildea la escena del juego hecha a mano sin regenerarla. También deja la lista de escenas
    /// del build correcta, evitando escenas viejas o rotas.
    /// </summary>
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/UnityJam.unity";

        [MenuItem("Subject626/Fijar escena del build")]
        public static void FixBuildScenes()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[Subject626] Lista de escenas del build fijada a: " + ScenePath);
        }

        [MenuItem("Subject626/Build Windows")]
        public static void BuildWindows()
        {
            FixBuildScenes();

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
