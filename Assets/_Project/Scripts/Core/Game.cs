using UnityEngine;

namespace Subject626
{
    public enum GameState { Playing, Paused, Escaped, Keypad, Between, Menu }

    /// <summary>
    /// Hub estatico: referencias a los sistemas y estado global. Reseteado por
    /// GameBootstrap al iniciar (importante para recargas de dominio en el editor).
    /// </summary>
    public static class Game
    {
        public static GameBootstrap Boot;
        public static Transform Player;
        public static Camera Cam;
        public static PlayerController Controller;
        public static PlayerCarry Carry;
        public static Room Room;
        public static HUD Hud;
        public static RevealController Reveal;
        public static RoundManager Rounds;
        public static Narrator Narrator;

        // Objetivo del juego: encontrar la llave abre la puerta "de verdad".
        public static bool HasKey;

        // Si es true, al (re)cargar la escena se arranca jugando directo, sin pasar por el menu.
        // No se limpia en Reset(): se consume una sola vez en GameBootstrap.Awake.
        public static bool StartInGame;

        static GameState state = GameState.Playing;
        public static GameState State { get { return state; } }
        public static bool IsPlaying { get { return state == GameState.Playing; } }

        public static void Reset()
        {
            Boot = null; Player = null; Cam = null; Controller = null; Carry = null;
            Room = null; Hud = null; Reveal = null; Rounds = null; Narrator = null;
            HasKey = false;
            state = GameState.Playing;
        }

        public static void SetState(GameState s)
        {
            state = s;
            bool freeCursor = (s != GameState.Playing);
            Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = freeCursor;
            Time.timeScale = (s == GameState.Paused || s == GameState.Between || s == GameState.Menu) ? 0f : 1f;
        }

        /// <summary>Salir de la aplicacion (seguro en editor y en build).</summary>
        public static void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
