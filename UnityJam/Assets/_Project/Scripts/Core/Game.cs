using UnityEngine;

namespace Subject626
{
    public enum GameState { Playing, Paused, Escaped, Keypad, Between }

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
            Time.timeScale = (s == GameState.Paused || s == GameState.Between) ? 0f : 1f;
        }
    }
}
