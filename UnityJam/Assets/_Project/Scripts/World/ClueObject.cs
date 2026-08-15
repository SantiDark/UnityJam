using UnityEngine;

namespace Subject626
{
    /// <summary>Pista del codigo: al examinarla (E) revela un digito y su posicion.</summary>
    public class ClueObject : MonoBehaviour, IInteractable
    {
        public int position;   // 1..4
        public int digit;      // 0..9

        public string Prompt() { return "E  Examinar"; }
        public bool CanInteract() { return Game.State == GameState.Playing; }

        public void Interact(PlayerInteractor by)
        {
            if (Game.Hud != null)
                Game.Hud.Toast("Codigo - posicion " + position + ": " + digit);
        }
    }
}
