using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// La llave. Aparece al levantar la alfombra; el jugador la recoge acercandose. Al agarrarla
    /// vuela hacia la camara, se achica y DESAPARECE. Con la llave, la puerta funciona de verdad.
    /// </summary>
    public class KeyItem : MonoBehaviour
    {
        public float pickupRadius = 1.4f;

        Vector3 startPos;
        Quaternion startRot;
        Vector3 startScale;
        bool collected;
        bool collecting;
        float collectT;
        Vector3 collectFrom;
        float spin;

        void Awake()
        {
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            startScale = transform.localScale;
        }

        public void CaptureStart()
        {
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            startScale = transform.localScale;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            if (collecting) { AnimateCollect(); return; }
            if (collected) return;

            // Girar y flotar para que se note.
            spin += Time.deltaTime * 90f;
            transform.localRotation = startRot * Quaternion.Euler(0f, spin, 0f);
            transform.localPosition = startPos + new Vector3(0f, Mathf.Sin(Time.time * 2.5f) * 0.05f, 0f);

            if (Game.Player != null &&
                Vector3.Distance(Game.Player.position, transform.position) < pickupRadius)
                BeginCollect();
        }

        void BeginCollect()
        {
            collecting = true;
            collectT = 0f;
            collectFrom = transform.position;
            Game.HasKey = true;
            if (Game.Hud != null)
            {
                Game.Hud.Toast("Agarraste la LLAVE. Ahora la puerta funciona.");
                Game.Hud.SetKey(true);
            }
            if (Game.Narrator != null) Game.Narrator.Event("key");
        }

        void AnimateCollect()
        {
            collectT += Time.deltaTime / 0.35f;
            Vector3 target = Game.Cam != null ? Game.Cam.transform.position : collectFrom;
            transform.position = Vector3.Lerp(collectFrom, target, collectT);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, collectT);
            if (collectT >= 1f)
            {
                collecting = false;
                collected = true;
                gameObject.SetActive(false);
            }
        }

        public void ResetState()
        {
            collected = false;
            collecting = false;
            Game.HasKey = false;
            transform.localPosition = startPos;
            transform.localRotation = startRot;
            transform.localScale = startScale;
            gameObject.SetActive(false); // escondida de nuevo hasta reabrir/levantar
            if (Game.Hud != null) Game.Hud.SetKey(false);
        }
    }
}
