using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>HUD: mira, prompt de interaccion, avisos, indicador de llave y ayuda inicial.</summary>
    public class HUD : MonoBehaviour
    {
        Text prompt;
        Text carryHint;
        Text keyLabel;
        Text toast;
        Text controls;
        Text foundLabel;
        float toastUntil;
        float controlsUntil;

        public void Build()
        {
            Canvas canvas = UIFactory.Canvas("HUD_Canvas", 20);

            // Mira central.
            Image dot = UIFactory.Panel(canvas.transform, new Color(1f, 1f, 1f, 0.8f),
                new Vector2(6f, 6f), Vector2.zero, new Vector2(0.5f, 0.5f));
            dot.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            prompt = UIFactory.Label(canvas.transform, "", new Vector2(900f, 40f), new Vector2(0f, -120f),
                new Vector2(0.5f, 0.5f), 26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            carryHint = UIFactory.Label(canvas.transform, "", new Vector2(900f, 30f), new Vector2(0f, 60f),
                new Vector2(0.5f, 0f), 20, FontStyle.Normal, TextAnchor.LowerCenter, new Color(0.9f, 0.9f, 0.9f));

            foundLabel = UIFactory.Label(canvas.transform, "Salidas encontradas: 0", new Vector2(760f, 44f), new Vector2(0f, -24f),
                new Vector2(0.5f, 1f), 30, FontStyle.Bold, TextAnchor.UpperCenter, MaterialLib.DevOrange);

            keyLabel = UIFactory.Label(canvas.transform, "", new Vector2(500f, 40f), new Vector2(30f, -30f),
                new Vector2(0f, 1f), 24, FontStyle.Bold, TextAnchor.UpperLeft, MaterialLib.Gold);

            toast = UIFactory.Label(canvas.transform, "", new Vector2(1200f, 44f), new Vector2(0f, -110f),
                new Vector2(0.5f, 1f), 26, FontStyle.Bold, TextAnchor.UpperCenter, new Color(1f, 0.95f, 0.7f));

            controls = UIFactory.Label(canvas.transform, "", new Vector2(700f, 200f), new Vector2(-30f, 30f),
                new Vector2(1f, 0f), 20, FontStyle.Normal, TextAnchor.LowerRight, new Color(0.85f, 0.85f, 0.88f));
            controls.text =
                "SUJETO DE PRUEBA N 626\n" +
                "Encontra TODAS las formas de salir de la habitacion.\n" +
                "Cada salida que uses se SELLA: vas a tener que buscar otra.\n\n" +
                "WASD mover   Shift correr   Espacio saltar\n" +
                "Clic izq agarrar/soltar   Clic der lanzar   Rueda acercar   R rotar\n" +
                "E interactuar   Esc pausa";
            controlsUntil = 20f;
        }

        public void SetPrompt(string s)
        {
            if (prompt != null) prompt.text = string.IsNullOrEmpty(s) ? "" : s;
        }

        public void ShowCarry(bool carrying)
        {
            if (carryHint != null)
                carryHint.text = carrying ? "Sosteniendo objeto  -  clic izq: soltar   clic der: lanzar   rueda: distancia   R: rotar" : "";
        }

        public void SetKey(bool has)
        {
            if (keyLabel != null) keyLabel.text = has ? "LLAVE: la tenes" : "";
        }

        public void SetFound(int n, int total)
        {
            // A proposito NO mostramos el total: que el jugador no sepa cuantas faltan.
            if (foundLabel != null) foundLabel.text = "Salidas encontradas: " + n;
        }

        public void Toast(string s)
        {
            if (toast == null) return;
            toast.text = s;
            toast.color = new Color(1f, 0.95f, 0.7f, 1f);
            toastUntil = Time.unscaledTime + 3.5f;
        }

        void Update()
        {
            if (toast != null && toast.text.Length > 0)
            {
                float left = toastUntil - Time.unscaledTime;
                if (left <= 0f) toast.text = "";
                else if (left < 1f) { Color c = toast.color; c.a = left; toast.color = c; }
            }
            if (controls != null && controls.text.Length > 0)
            {
                controlsUntil -= Time.unscaledDeltaTime;
                if (controlsUntil <= 0f) controls.text = "";
                else if (controlsUntil < 3f) { Color c = controls.color; c.a = controlsUntil / 3f; controls.color = c; }
            }
        }
    }
}
