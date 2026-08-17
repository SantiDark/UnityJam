using UnityEngine;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Raycast desde la camara para detectar e interactuar con el mundo (tecla E).</summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float _reach = 2.6f;
        private Camera _camera;
        private IInteractable _currentInteractable;

        private LayerMask _interactableLayer;

        void Start() 
        { 
            _camera = Game.Cam;
            _interactableLayer = LayerMask.GetMask("Interactable");
        }

        void Update()
        {
            if (_camera == null) _camera = Game.Cam;
            if (_camera == null) return;

            _currentInteractable = null;
            if (Game.State == GameState.Playing)
            {
                RaycastHit hit;
                Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

                if (Physics.Raycast(ray, out hit, _reach, _interactableLayer, QueryTriggerInteraction.Collide))
                {
                    IInteractable it = hit.collider.GetComponentInParent<IInteractable>();
                    if (it != null && it.CanInteract()) _currentInteractable = it;
                }
            }

            if (Game.Hud != null)
                Game.Hud.SetPrompt(_currentInteractable != null ? _currentInteractable.Prompt() : null);

            Keyboard k = Keyboard.current;
            
            if (_currentInteractable != null && k != null && k.eKey.wasPressedThisFrame && Game.State == GameState.Playing)
                _currentInteractable.Interact(this);
        }
    }
}
