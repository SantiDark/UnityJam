using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Panel rajado: se rompe a golpes de objetos LANZADOS con fuerza (varios impactos).
    /// Al romperse habilita la salida detras. Si ya lo usaste, vuelve REFORZADO (irrompible).
    /// </summary>
    public class BreakablePanel : MonoBehaviour, IExit
    {
        public int hitsToBreak = 4;
        public float minHitSpeed = 5.5f;
        public GameObject exitToEnable;
        public Renderer rend;

        int hits;
        float lastHitTime;
        bool broken;
        bool sealedOff;

        public ExitId Id { get { return ExitId.Panel; } }
        public float Progress { get { return Mathf.Clamp01((float)hits / hitsToBreak); } }

        void OnCollisionEnter(Collision c)
        {
            if (broken || sealedOff) return;
            if (c.rigidbody == null) return;
            if (c.gameObject.GetComponentInParent<Grabbable>() == null) return;
            if (c.relativeVelocity.magnitude < minHitSpeed) return;
            if (Time.time - lastHitTime < 0.15f) return;

            lastHitTime = Time.time;
            hits++;
            UpdateVisual();
            if (Game.Hud != null)
                Game.Hud.Toast(hits >= hitsToBreak ? "El panel se rompe." : "El panel se raja mas (" + hits + "/" + hitsToBreak + ")");

            if (hits >= hitsToBreak) Break();
        }

        void UpdateVisual()
        {
            if (rend == null) return;
            float t = Progress;
            rend.sharedMaterial = MaterialLib.Solid(Color.Lerp(MaterialLib.WallPaper, new Color(0.15f, 0.13f, 0.12f), t), 0.1f);
        }

        void Break()
        {
            broken = true;
            if (exitToEnable != null) exitToEnable.SetActive(true);
            gameObject.SetActive(false);
        }

        public void ResetForRound(bool sealedOff)
        {
            this.sealedOff = sealedOff;
            broken = false;
            hits = 0;
            gameObject.SetActive(true);
            if (exitToEnable != null)
            {
                ExitTrigger et = exitToEnable.GetComponent<ExitTrigger>();
                if (et != null) et.ResetState();
                exitToEnable.SetActive(false);
            }
            if (rend != null)
                rend.sharedMaterial = sealedOff
                    ? MaterialLib.Solid(MaterialLib.Metal, 0.3f, 0.7f)   // reforzado, se nota
                    : MaterialLib.Solid(MaterialLib.WallPaper, 0.1f);
        }
    }
}
