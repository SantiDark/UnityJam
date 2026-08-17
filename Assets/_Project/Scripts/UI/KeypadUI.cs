using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Mono.Cecil;

namespace Subject626
{
    /// <summary>Ingreso del codigo de 4 digitos. Teclas 0-9 escriben, Enter valida, Backspace borra, Esc cancela.</summary>
    public class KeypadUI : MonoBehaviour
    {
        public static KeypadUI Instance;

        Canvas canvas;
        Text display;
        Text feedback;
        string code = "0000";
        string entry = "";
        float shakeUntil;

        private List<string> _usedCodes = new List<string>();

        private bool _fiveMatchingAttempts;
        public bool FiveMatchingAttempts => _fiveMatchingAttempts;

        void Awake() { Instance = this; }

        public void Build()
        {
            canvas = UIFactory.Canvas("Keypad_Canvas", 50);
            UIFactory.Stretch(canvas.transform, new Color(0.03f, 0.03f, 0.05f, 0.85f));

            UIFactory.Label(canvas.transform, "TECLADO DE SEGURIDAD",
                new Vector2(900f, 50f), new Vector2(0f, 200f), new Vector2(0.5f, 0.5f),
                40, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Label(canvas.transform, "Ingresa el codigo de 4 digitos (las pistas estan en la sala)",
                new Vector2(1100f, 40f), new Vector2(0f, 150f), new Vector2(0.5f, 0.5f),
                22, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.85f));

            display = UIFactory.Label(canvas.transform, "_ _ _ _",
                new Vector2(900f, 90f), new Vector2(0f, 40f), new Vector2(0.5f, 0.5f),
                72, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.Gold);

            feedback = UIFactory.Label(canvas.transform, "",
                new Vector2(900f, 40f), new Vector2(0f, -60f), new Vector2(0.5f, 0.5f),
                26, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.Red);

            UIFactory.Label(canvas.transform, "0-9 escribir    Backspace borrar    Enter validar    Esc salir",
                new Vector2(1100f, 40f), new Vector2(0f, -160f), new Vector2(0.5f, 0.5f),
                20, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.8f));

            canvas.gameObject.SetActive(false);
        }

        public void Open(string code)
        {
            this.code = code;
            entry = "";
            if (feedback != null) feedback.text = "";
            Refresh();
            if (canvas != null) canvas.gameObject.SetActive(true);
            Game.SetState(GameState.Keypad);
        }

        void Close()
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
            Game.SetState(GameState.Playing);
        }

        void Update()
        {
            if (Game.State != GameState.Keypad) return;
            Keyboard k = Keyboard.current;
            if (k == null) return;

            if (k.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (k.backspaceKey.wasPressedThisFrame && entry.Length > 0)
            {
                entry = entry.Substring(0, entry.Length - 1);
                Refresh();
            }
            if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)
                Submit();

            if (entry.Length < 4)
            {
                int d = PressedDigit(k);
                if (d >= 0) { entry += d.ToString(); Refresh(); }
            }
        }

        int PressedDigit(Keyboard k)
        {
            KeyControl[] top = { k.digit0Key, k.digit1Key, k.digit2Key, k.digit3Key, k.digit4Key,
                                 k.digit5Key, k.digit6Key, k.digit7Key, k.digit8Key, k.digit9Key };
            KeyControl[] pad = { k.numpad0Key, k.numpad1Key, k.numpad2Key, k.numpad3Key, k.numpad4Key,
                                 k.numpad5Key, k.numpad6Key, k.numpad7Key, k.numpad8Key, k.numpad9Key };
            for (int i = 0; i < 10; i++)
                if (top[i].wasPressedThisFrame || pad[i].wasPressedThisFrame) return i;
            return -1;
        }

        void Refresh()
        {
            if (display == null) return;
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                s += (i < entry.Length) ? entry[i].ToString() : "_";
                if (i < 3) s += " ";
            }
            display.text = s;
        }

        void Submit()
        {
            if (entry.Length < 4) { if (feedback != null) feedback.text = "Faltan digitos"; return; }
            if (entry == code)
            {
                if (canvas != null) canvas.gameObject.SetActive(false);
                if (Game.Rounds != null) Game.Rounds.Escape(ExitId.Keypad);
            }
            else
            {
                if (feedback != null) feedback.text = "Codigo incorrecto";

                _usedCodes.Add(entry);
                _fiveMatchingAttempts = _usedCodes.GroupBy(x => x).Any(group => group.Count() >= 5);

                entry = "";
                Refresh();
            }
        }
    }
}
