using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Menu de pausa (Esc): reanudar, reiniciar, volver al menu principal o salir.</summary>
    public class PauseMenu : MonoBehaviour
    {
        Canvas canvas;
        bool paused;

        public void Build()
        {
            canvas = UIFactory.Canvas("PauseMenu_Canvas", 55);
            UIFactory.Stretch(canvas.transform, new Color(0.03f, 0.03f, 0.05f, 0.82f));

            UIFactory.Label(canvas.transform, "PAUSA",
                new Vector2(800f, 90f), new Vector2(0f, 220f), new Vector2(0.5f, 0.5f),
                64, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            UIFactory.Button(canvas.transform, "REANUDAR", new Vector2(360f, 60f), new Vector2(0f, 90f),
                new Vector2(0.5f, 0.5f), Resume);
            UIFactory.Button(canvas.transform, "REINICIAR", new Vector2(360f, 60f), new Vector2(0f, 15f),
                new Vector2(0.5f, 0.5f), Restart);
            UIFactory.Button(canvas.transform, "MENU PRINCIPAL", new Vector2(360f, 60f), new Vector2(0f, -60f),
                new Vector2(0.5f, 0.5f), ToMainMenu);
            UIFactory.Button(canvas.transform, "SALIR", new Vector2(360f, 60f), new Vector2(0f, -135f),
                new Vector2(0.5f, 0.5f), Game.QuitApp);

            canvas.gameObject.SetActive(false);
        }

        void Update()
        {
            Keyboard k = Keyboard.current;
            if (k == null || !k.escapeKey.wasPressedThisFrame) return;

            if (Game.State == GameState.Playing) Pause();
            else if (Game.State == GameState.Paused && paused) Resume();
        }

        void Pause()
        {
            paused = true;
            if (canvas != null) canvas.gameObject.SetActive(true);
            Game.SetState(GameState.Paused);
        }

        void Resume()
        {
            paused = false;
            if (canvas != null) canvas.gameObject.SetActive(false);
            Game.SetState(GameState.Playing);
        }

        void Restart()
        {
            paused = false;
            Game.StartInGame = true;              // recargar directo en juego, sin menu
            if (Game.Boot != null) Game.Boot.Restart();
        }

        void ToMainMenu()
        {
            paused = false;
            Game.StartInGame = false;             // recargar mostrando el menu
            if (Game.Boot != null) Game.Boot.Restart();
        }
    }
}
