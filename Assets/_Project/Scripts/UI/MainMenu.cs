using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>
    /// Menu principal: aparece al iniciar. El juego (mundo, jugador, sistemas) recien se construye
    /// cuando se aprieta JUGAR, para que nada del in-game corra durante el menu.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        Canvas canvas;
        System.Action onPlay;

        public void Build(System.Action onPlay)
        {
            this.onPlay = onPlay;

            canvas = UIFactory.Canvas("MainMenu_Canvas", 60);
            // Fondo opaco (no hay camara del jugador todavia).
            UIFactory.Stretch(canvas.transform, new Color(0.05f, 0.05f, 0.07f, 1f));

            UIFactory.Label(canvas.transform, "SUBJECT 626",
                new Vector2(1400f, 120f), new Vector2(0f, 200f), new Vector2(0.5f, 0.5f),
                90, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Label(canvas.transform, "Salga de la habitacion. De todas las formas posibles.",
                new Vector2(1400f, 40f), new Vector2(0f, 120f), new Vector2(0.5f, 0.5f),
                26, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.85f));

            UIFactory.Button(canvas.transform, "JUGAR", new Vector2(360f, 64f), new Vector2(0f, -10f),
                new Vector2(0.5f, 0.5f), Play);
            UIFactory.Button(canvas.transform, "SALIR", new Vector2(360f, 64f), new Vector2(0f, -90f),
                new Vector2(0.5f, 0.5f), Game.QuitApp);

            UIFactory.Label(canvas.transform, "CONTROL 626",
                new Vector2(500f, 30f), new Vector2(-30f, 20f), new Vector2(1f, 0f),
                18, FontStyle.Bold, TextAnchor.LowerRight, MaterialLib.DevOrange);
        }

        public void Show()
        {
            if (canvas != null) canvas.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
        }

        void Play()
        {
            Hide();
            if (onPlay != null) onPlay();
        }
    }
}
