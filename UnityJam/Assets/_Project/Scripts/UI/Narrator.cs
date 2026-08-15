using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>
    /// La voz del que dirige el ensayo (CONTROL). Da la bienvenida, comenta lo que hacés y
    /// suelta observaciones cuando estás pensando. Aporta personalidad e inmersion.
    /// Subtitulos abajo, con cola simple y cooldowns para no spamear.
    /// </summary>
    public class Narrator : MonoBehaviour
    {
        Image panel;
        Text speaker;
        Text line;

        readonly Queue<string> queue = new Queue<string>();
        readonly HashSet<string> firedOnce = new HashSet<string>();
        string current;
        float clearAt;
        float nextIdle;
        int idleIdx;

        static readonly string[] Welcome =
        {
            "Bienvenido de vuelta, sujeto 626. Comienza la sesion.",
            "Su tarea es simple: salga de la habitacion. Nosotros observamos como.",
        };

        static readonly string[] Idle =
        {
            "Los sensores dicen que sigue adentro. Curioso.",
            "Tomese su tiempo. Nosotros tenemos de sobra.",
            "Cada objeto de esta sala esta ahi por una razon.",
            "La mayoria de los sujetos ya habria probado algo.",
            "Rendirse tambien es un dato util, sabe.",
            "La salida no siempre es la mas obvia.",
            "No toca lo que esperabamos que tocara. Anotado.",
            "Respire. Piense. Y despues rompa algo, si hace falta.",
        };

        public void Build()
        {
            Canvas canvas = UIFactory.Canvas("Narrator_Canvas", 25);

            panel = UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.55f),
                new Vector2(1300f, 96f), new Vector2(0f, 190f), new Vector2(0.5f, 0f));
            speaker = UIFactory.Label(panel.transform, "CONTROL 626", new Vector2(1240f, 24f), new Vector2(0f, 30f),
                new Vector2(0.5f, 0.5f), 18, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.DevOrange);
            line = UIFactory.Label(panel.transform, "", new Vector2(1240f, 56f), new Vector2(0f, -8f),
                new Vector2(0.5f, 0.5f), 24, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.95f));

            panel.gameObject.SetActive(false);
            nextIdle = 12f;
        }

        void Start()
        {
            foreach (string w in Welcome) queue.Enqueue(w);
        }

        /// <summary>Comenta un evento del juego. `once`=solo la primera vez.</summary>
        public void Event(string id)
        {
            string[] opts = LinesFor(id);
            if (opts == null || opts.Length == 0) return;
            if (OnceEvents.Contains(id))
            {
                if (firedOnce.Contains(id)) return;
                firedOnce.Add(id);
            }
            string pick = opts[Random.Range(0, opts.Length)];
            // Los eventos tienen prioridad: van adelante y cortan lo que estaba.
            current = null;
            clearAt = 0f;
            queue.Clear();
            queue.Enqueue(pick);
            nextIdle = Time.unscaledTime + 18f; // no idle justo despues de un evento
        }

        static readonly HashSet<string> OnceEvents = new HashSet<string>
        { "grab", "throw", "elevated" };

        static string[] LinesFor(string id)
        {
            switch (id)
            {
                case "grab": return new[] { "Bien. Manipular el entorno es parte de la prueba." };
                case "throw": return new[] { "Agresivo. Eso tambien lo anotamos." };
                case "elevated": return new[] { "Sube. Veamos hasta donde llega." };
                case "pit": return new[] {
                    "Ups. De vuelta al principio. Sin rencores.",
                    "La gravedad tambien es parte del ensayo." };
                case "door_troll": return new[] {
                    "La puerta. Que original. La reiniciamos por usted.",
                    "Insiste con la puerta. Adorable." };
                case "key": return new[] {
                    "Encontro algo que escondimos bien. Impresionante.",
                    "Esa llave no deberia haber estado a su alcance. Bien." };
                case "sealed": return new[] {
                    "Esa salida ya la conoce. La cerramos. Busque otra.",
                    "Repetir no cuenta, sujeto 626." };
            }
            return null;
        }

        void Update()
        {
            float now = Time.unscaledTime;

            // Mostrar / avanzar la cola.
            if (current == null && queue.Count > 0)
            {
                current = queue.Dequeue();
                if (line != null) line.text = current;
                if (panel != null) panel.gameObject.SetActive(true);
                clearAt = now + Mathf.Clamp(current.Length * 0.055f, 2.5f, 5.5f);
            }
            else if (current != null && now >= clearAt)
            {
                current = null;
                if (panel != null) panel.gameObject.SetActive(false);
                nextIdle = now + Random.Range(24f, 40f);
            }

            // Detecta cuando el jugador se subio a algo (apilando cajas).
            if (Game.State == GameState.Playing && Game.Player != null && Game.Player.position.y > 1.3f)
                Event("elevated");

            // Observaciones sueltas mientras jugás y no hay nada en pantalla.
            if (current == null && queue.Count == 0 && Game.State == GameState.Playing && now >= nextIdle)
            {
                queue.Enqueue(Idle[idleIdx % Idle.Length]);
                idleIdx++;
                nextIdle = now + Random.Range(28f, 46f);
            }
        }
    }
}
