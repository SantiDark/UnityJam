using UnityEngine;

namespace Subject626
{
    /// <summary>
    /// La alfombra esconde la llave debajo. No hay ninguna pista: hay que sospechar de la
    /// alfombra, mirarla y levantarla (E). Es la solucion mas dificil de descubrir.
    /// </summary>
    public class RugCover : MonoBehaviour, IInteractable
    {
        public KeyItem key;

        Vector3 startPos;
        Quaternion startRot;
        bool lifted;

        void Awake()
        {
            startPos = transform.localPosition;
            startRot = transform.localRotation;
        }

        public string Prompt() { return lifted ? "" : "E  Levantar la alfombra"; }
        public bool CanInteract() { return !lifted && Game.State == GameState.Playing; }

        public void Interact(PlayerInteractor by)
        {
            if (lifted) return;
            lifted = true;
            transform.localPosition = startPos + new Vector3(1.7f, 0f, 0f);
            transform.localRotation = startRot * Quaternion.Euler(0f, 0f, 22f);
            if (key != null) key.gameObject.SetActive(true);
            if (Game.Hud != null) Game.Hud.Toast("Debajo de la alfombra habia una llave.");
        }

        public void ResetState()
        {
            lifted = false;
            transform.localPosition = startPos;
            transform.localRotation = startRot;
        }
    }
}
