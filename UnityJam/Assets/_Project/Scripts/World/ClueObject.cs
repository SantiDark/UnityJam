using UnityEngine;

namespace Subject626
{
    /// <summary>Pista del codigo: al examinarla (E) revela un digito y su posicion.</summary>
    public class ClueObject : MonoBehaviour, IInteractable
    {
        public int position;   // 1..4
        public int digit;      // 0..9

        void Awake()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) box = gameObject.AddComponent<BoxCollider>();

            // Las pistas son placas finitas sobre muebles/paredes. Hacemos un trigger un poco más
            // grande para que la E no quede bloqueada por el collider del sillón o la biblioteca.
            box.isTrigger = true;
            box.size = new Vector3(1.35f, 1.35f, 6f);
            box.center = Vector3.zero;
        }

        public string Prompt() { return "E  Examinar"; }
        public bool CanInteract() { return Game.State == GameState.Playing; }

        public void Interact(PlayerInteractor by)
        {
            if (Game.Hud != null)
                Game.Hud.Toast("Codigo - posicion " + position + ": " + digit);
        }
    }
}
