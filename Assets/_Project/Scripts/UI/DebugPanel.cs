using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Panel de debug (F1): probar las tres soluciones y utilidades al instante.</summary>
    public class DebugPanel : MonoBehaviour
    {
        Canvas canvas;
        bool open;
        float y;

        void Start()
        {
            canvas = UIFactory.Canvas("Debug_Canvas", 60);
            UIFactory.Stretch(canvas.transform, new Color(0.05f, 0.05f, 0.07f, 0.92f));

            UIFactory.Label(canvas.transform, "DEBUG  -  Subject 626",
                new Vector2(600f, 40f), new Vector2(40f, -30f), new Vector2(0f, 1f),
                30, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
            UIFactory.Label(canvas.transform,
                "Seis soluciones para salir:\n" +
                "1) Apilar cajas, subirse y sacar el POSTER del techo.\n" +
                "2) Cruzar la PARED derecha (sin collider) y saltar CON carrera a la plataforma.\n" +
                "3) Encontrar la LLAVE escondida DEBAJO de la alfombra (E) y usar la PUERTA.\n" +
                "4) Romper el PANEL rajado del fondo lanzandole objetos (varios golpes fuertes).\n" +
                "5) Juntar/apilar peso en la PLACA de presion -> abre una compuerta en el piso.\n" +
                "6) Buscar las 4 PISTAS (E) y meter el CODIGO en el TECLADO al lado de la puerta.\n" +
                "Abrir la puerta SIN llave reinicia la sala.",
                new Vector2(1000f, 240f), new Vector2(40f, -80f), new Vector2(0f, 1f),
                19, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.9f));

            y = -370f;
            Button("Dar llave", delegate { Game.HasKey = true; if (Game.Hud != null) { Game.Hud.SetKey(true); Game.Hud.Toast("Llave otorgada (debug)."); } });
            Button("Mostrar codigo del teclado", delegate {
                KeypadController kc = Object.FindFirstObjectByType<KeypadController>();
                if (kc != null && Game.Hud != null) Game.Hud.Toast("Codigo (debug): " + kc.code);
            });
            Button("Recomenzar ronda (soft)", delegate { if (Game.Rounds != null) Game.Rounds.SoftReset(); });
            Button("TP frente a la puerta", delegate { if (Game.Controller != null) Game.Controller.Teleport(new Vector3(0f, 0.1f, 4.6f), 0f); });
            Button("TP elevado (poster)", delegate { if (Game.Controller != null) Game.Controller.Teleport(new Vector3(-0.6f, 1.4f, -0.6f), 180f); });
            Button("TP borde pared falsa", delegate { if (Game.Controller != null) Game.Controller.Teleport(new Vector3(4.4f, 0.1f, 0f), 90f); });
            Button("Registrar salida PUERTA", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.KeyDoor); });
            Button("Registrar salida PARED", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.FalseWall); });
            Button("Registrar salida POSTER", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Poster); });
            Button("Registrar salida PANEL", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Panel); });
            Button("Registrar salida COMPUERTA", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Plate); });
            Button("Registrar salida TECLADO", delegate { if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Keypad); });
            Button("Forzar FINAL (reveal)", delegate { if (Game.Reveal != null) Game.Reveal.FinalReveal(); });
            Button("Reiniciar escena", delegate { if (Game.Boot != null) Game.Boot.Restart(); });

            canvas.gameObject.SetActive(false);
        }

        void Button(string label, UnityEngine.Events.UnityAction action)
        {
            Image img = UIFactory.Panel(canvas.transform, new Color(0.18f, 0.18f, 0.22f, 0.98f),
                new Vector2(360f, 44f), new Vector2(40f, y), new Vector2(0f, 1f));
            Button b = img.gameObject.AddComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = new Color(0.2f, 0.2f, 0.24f, 1f);
            cb.highlightedColor = new Color(0.32f, 0.32f, 0.4f, 1f);
            cb.pressedColor = new Color(0.12f, 0.12f, 0.15f, 1f);
            b.colors = cb;
            UIFactory.Label(img.transform, label, new Vector2(360f, 44f), Vector2.zero,
                new Vector2(0.5f, 0.5f), 20, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
            b.onClick.AddListener(action);
            y -= 52f;
        }

        void Update()
        {
            Keyboard k = Keyboard.current;
            if (k != null && k.f1Key.wasPressedThisFrame)
                Toggle();
        }

        void Toggle()
        {
            open = !open;
            if (canvas != null) canvas.gameObject.SetActive(open);
            // Solo tocamos el estado si estabamos jugando (no pisar el reveal).
            if (open)
            {
                if (Game.State == GameState.Playing) Game.SetState(GameState.Paused);
            }
            else
            {
                if (Game.State == GameState.Paused) Game.SetState(GameState.Playing);
            }
        }
    }
}
