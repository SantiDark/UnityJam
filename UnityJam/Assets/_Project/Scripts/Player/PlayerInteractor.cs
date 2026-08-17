using UnityEngine;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Raycast desde la camara para detectar e interactuar con el mundo (tecla E).</summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float _reach = 2.6f;
        [SerializeField] private float _aimAssistRadius = 0.14f;
        [SerializeField] private float _occlusionForgiveness = 0.45f;

        private Camera _camera;
        private IInteractable _currentInteractable;

        private LayerMask _interactableLayer;

        void Start()
        {
            _camera = Game.Cam;
            int mask = LayerMask.GetMask("Interactable");
            _interactableLayer = (mask != 0) ? mask : Physics.DefaultRaycastLayers;
        }

        void Update()
        {
            if (_camera == null) _camera = Game.Cam;
            if (_camera == null) return;

            _currentInteractable = null;
            if (Game.State == GameState.Playing)
            {
                Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
                _currentInteractable = FindInteractable(ray);
            }

            if (Game.Hud != null)
                Game.Hud.SetPrompt(_currentInteractable != null ? _currentInteractable.Prompt() : null);

            Keyboard k = Keyboard.current;

            if (_currentInteractable != null && k != null && k.eKey.wasPressedThisFrame && Game.State == GameState.Playing)
                _currentInteractable.Interact(this);
        }

        IInteractable FindInteractable(Ray ray)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                ray,
                _aimAssistRadius,
                _reach,
                _interactableLayer,
                QueryTriggerInteraction.Collide);

            if (hits == null || hits.Length == 0) return null;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            float firstBlockingDistance = float.PositiveInfinity;
            foreach (RaycastHit hit in hits)
            {
                Collider hitCollider = hit.collider;
                if (hitCollider == null || IsOwnCollider(hitCollider)) continue;

                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    if (float.IsPositiveInfinity(firstBlockingDistance) ||
                        hit.distance <= firstBlockingDistance + _occlusionForgiveness)
                    {
                        return interactable;
                    }
                }

                if (!hitCollider.isTrigger && float.IsPositiveInfinity(firstBlockingDistance))
                    firstBlockingDistance = hit.distance;
            }

            return null;
        }

        bool IsOwnCollider(Collider hitCollider)
        {
            Transform hitTransform = hitCollider.transform;
            return hitTransform == transform || hitTransform.IsChildOf(transform);
        }
    }
}
