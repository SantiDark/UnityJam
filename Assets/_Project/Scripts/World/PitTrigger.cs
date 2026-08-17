using UnityEngine;

namespace Subject626
{
    /// <summary>Si el jugador cae al hueco entre la sala y la plataforma, vuelve a la entrada (sin resetear el resto).</summary>
    public class PitTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (Game.Room != null) Game.Room.RespawnPlayer();
            if (Game.Hud != null) Game.Hud.Toast("Casi. Volves a la entrada.");
            if (Game.Narrator != null) Game.Narrator.Event("pit");
        }
    }
}
