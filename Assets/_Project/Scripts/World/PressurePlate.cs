using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Placa de presion: suma la masa apoyada encima; al umbral abre una compuerta (salida).
    /// Si ya usaste esta salida, queda SELLADA (compuerta soldada) y no vuelve a abrir.
    /// </summary>
    public class PressurePlate : MonoBehaviour, IExit
    {
        public float massThreshold = 7.5f;
        public Vector3 halfExtents = new Vector3(0.9f, 0.15f, 0.9f);
        public Renderer plateRend;
        public GameObject exitToEnable;
        public Transform hatchCover;

        bool open;
        bool sealedOff;
        Vector3 coverStart;
        readonly Collider[] buffer = new Collider[16];

        public ExitId Id { get { return ExitId.Plate; } }

        void Awake()
        {
            if (hatchCover != null) coverStart = hatchCover.localPosition;
        }

        void FixedUpdate()
        {
            if (open || sealedOff) return;

            Vector3 center = transform.position + Vector3.up * halfExtents.y;
            int n = Physics.OverlapBoxNonAlloc(center, halfExtents, buffer, transform.rotation, ~0, QueryTriggerInteraction.Ignore);
            float sum = 0f;
            for (int i = 0; i < n; i++)
            {
                Rigidbody rb = buffer[i] != null ? buffer[i].attachedRigidbody : null;
                if (rb == null) continue;
                if (rb.GetComponent<Grabbable>() == null) continue;
                if (rb.linearVelocity.magnitude > 1.5f) continue;
                sum += rb.mass;
            }

            if (plateRend != null)
            {
                plateRend.material.color = sum > 0f
                    ? new Color32(130, 111, 93, 255)
                    : new Color32(130, 111, 93, 255);
            }

            float ratio = Mathf.Clamp01(sum / massThreshold);
            //if (plateRend != null)
            //    plateRend.sharedMaterial = MaterialLib.Emissive(Color.Lerp(MaterialLib.Red, MaterialLib.Green, ratio), 0.8f + ratio);

            if (sum >= massThreshold) Open();
        }

        void Open()
        {
            open = true;
            if (hatchCover != null) hatchCover.localPosition = coverStart + new Vector3(0f, -0.05f, 1.6f);
            if (exitToEnable != null) exitToEnable.SetActive(true);
            if (Game.Hud != null) Game.Hud.Toast("Se abrio una compuerta en el piso.");
        }

        public void ResetForRound(bool sealedOff)
        {
            this.sealedOff = sealedOff;
            open = false;
            if (hatchCover != null) hatchCover.localPosition = coverStart;
            if (exitToEnable != null)
            {
                ExitTrigger et = exitToEnable.GetComponent<ExitTrigger>();
                if (et != null) et.ResetState();
                exitToEnable.SetActive(false);
            }
            //if (plateRend != null)
            //    plateRend.sharedMaterial = sealedOff
            //        ? MaterialLib.Solid(MaterialLib.Metal, 0.3f, 0.7f)     // soldada
            //        : MaterialLib.Emissive(MaterialLib.Red, 0.9f);
        }
    }
}
