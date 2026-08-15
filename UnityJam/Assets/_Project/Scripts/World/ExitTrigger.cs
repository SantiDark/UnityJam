using UnityEngine;

namespace Subject626
{
    /// <summary>Volumen de salida: cuando el jugador lo pisa, registra ese escape en el test.</summary>
    public class ExitTrigger : MonoBehaviour
    {
        public ExitId id;
        bool fired;

        void OnTriggerEnter(Collider other)
        {
            if (fired) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            fired = true;
            if (Game.Rounds != null) Game.Rounds.Escape(id);
        }

        public void ResetState() { fired = false; }
    }
}
