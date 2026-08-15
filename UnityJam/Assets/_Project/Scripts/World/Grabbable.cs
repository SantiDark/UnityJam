using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// Objeto fisico que el jugador puede levantar, mover y apilar. Recuerda su pose
    /// inicial para que la sala se pueda "reiniciar" (troll de la puerta).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        Rigidbody rb;
        Vector3 startPos;
        Quaternion startRot;
        bool held;

        public bool IsHeld { get { return held; } }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            startPos = transform.position;
            startRot = transform.rotation;
        }

        /// <summary>Vuelve a capturar la pose actual como "inicial" (tras armar la sala).</summary>
        public void CaptureStart()
        {
            startPos = transform.position;
            startRot = transform.rotation;
        }

        public void OnGrabbed(PlayerCarry by)
        {
            held = true;
            if (Game.Narrator != null) Game.Narrator.Event("grab");
        }
        public void OnReleased() { held = false; }

        public void ResetPose()
        {
            held = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
                rb.linearDamping = 0.2f;
                rb.angularDamping = 0.5f;
            }
            transform.SetPositionAndRotation(startPos, startRot);
        }
    }
}
