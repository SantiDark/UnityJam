using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>
    /// El FINAL de verdad: solo se dispara cuando encontraste las SEIS salidas. Cambia el arte
    /// de golpe a greybox y felicita al sujeto de prueba por resolver el test completo.
    /// </summary>
    public class RevealController : MonoBehaviour
    {
        Light sun;
        List<Light> roomLights;
        Transform backstage;
        Vector3 spawn;
        float yaw;

        Canvas canvas;
        Image flash;
        Text methodLine;
        bool done;

        public void Build(Light sun, List<Light> roomLights, Transform backstage, Vector3 spawn, float yaw)
        {
            this.sun = sun;
            this.roomLights = roomLights;
            this.backstage = backstage;
            this.spawn = spawn;
            this.yaw = yaw;

            canvas = UIFactory.Canvas("Reveal_Canvas", 40);
            canvas.gameObject.SetActive(false);

            // Franja superior con el titulo del "informe".
            UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.72f),
                new Vector2(3000f, 220f), new Vector2(0f, 0f), new Vector2(0.5f, 1f));
            UIFactory.Label(canvas.transform, "BIEN HECHO, SUJETO DE PRUEBA N 626",
                new Vector2(1600f, 60f), new Vector2(0f, -55f), new Vector2(0.5f, 1f),
                46, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Label(canvas.transform, "Encontraste TODAS las salidas.",
                new Vector2(1600f, 40f), new Vector2(0f, -120f), new Vector2(0.5f, 1f),
                28, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.85f));

            // Franja inferior: cierre + recordatorio del tema.
            UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.72f),
                new Vector2(3000f, 210f), new Vector2(0f, 0f), new Vector2(0.5f, 0f));
            methodLine = UIFactory.Label(canvas.transform, "Todas las salidas registradas. Sos un caso excepcional, sujeto 626.",
                new Vector2(1700f, 40f), new Vector2(0f, 150f), new Vector2(0.5f, 0f),
                28, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.DevOrange);
            UIFactory.Label(canvas.transform,
                "POSTER  /  PARED falsa  /  PUERTA con llave  /  PANEL  /  COMPUERTA  /  TECLADO.",
                new Vector2(1700f, 40f), new Vector2(0f, 95f), new Vector2(0.5f, 0f),
                21, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.82f, 0.82f, 0.86f));
            UIFactory.Label(canvas.transform, "R  -  volver a empezar",
                new Vector2(1700f, 40f), new Vector2(0f, 40f), new Vector2(0.5f, 0f),
                24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

            // Flash blanco de transicion.
            flash = UIFactory.Stretch(canvas.transform, new Color(1f, 1f, 1f, 0f));
            flash.raycastTarget = false;
        }

        public void FinalReveal()
        {
            if (done) return;
            done = true;

            if (Game.Carry != null) Game.Carry.ForceDrop();

            // Cambio de arte: apago las luces calidas y paso a look plano de prototipo.
            if (roomLights != null)
                foreach (Light l in roomLights) if (l != null) l.enabled = false;
            RenderSettings.ambientLight = new Color(0.55f, 0.56f, 0.60f);
            if (sun != null)
            {
                sun.color = Color.white;
                sun.intensity = 1.1f;
                sun.transform.rotation = Quaternion.Euler(60f, 20f, 0f);
            }

            if (backstage != null) backstage.gameObject.SetActive(true);
            if (Game.Controller != null) Game.Controller.Teleport(spawn, yaw);

            if (canvas != null) canvas.gameObject.SetActive(true);
            if (flash != null) flash.color = new Color(1f, 1f, 1f, 1f);

            Game.SetState(GameState.Escaped);
        }

        void Update()
        {
            if (flash != null && flash.color.a > 0f)
            {
                Color c = flash.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.6f);
                flash.color = c;
            }
        }
    }
}
