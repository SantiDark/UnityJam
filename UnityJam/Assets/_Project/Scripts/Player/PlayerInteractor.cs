using UnityEngine;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Raycast desde la camara para detectar e interactuar con el mundo (tecla E).</summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float reach = 2.6f;
        Camera cam;
        IInteractable current;

        void Start() { cam = Game.Cam; }

        void Update()
        {
            if (cam == null) cam = Game.Cam;
            if (cam == null) return;

            current = null;
            if (Game.State == GameState.Playing)
            {
                RaycastHit hit;
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(ray, out hit, reach, ~0, QueryTriggerInteraction.Collide))
                {
                    IInteractable it = hit.collider.GetComponentInParent<IInteractable>();
                    if (it != null && it.CanInteract()) current = it;
                }
            }

            if (Game.Hud != null)
                Game.Hud.SetPrompt(current != null ? current.Prompt() : null);

            Keyboard k = Keyboard.current;
            if (current != null && k != null && k.eKey.wasPressedThisFrame && Game.State == GameState.Playing)
                current.Interact(this);
        }
    }
}
