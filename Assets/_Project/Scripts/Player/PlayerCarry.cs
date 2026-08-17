using UnityEngine;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>
    /// Agarrar / cargar / apilar / lanzar objetos fisicos (estilo mano de fisica).
    /// Clic izq: agarrar o soltar. Clic der: lanzar. Rueda: acercar/alejar. R: rotar 90 grados.
    /// El objeto sostenido sigue colisionando con el MUNDO (se mueve por velocidad) asi se puede
    /// apoyar y APILAR, pero NO colisiona con el jugador: no se puede "surfear" la caja para volar.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        [SerializeField] private float _reach = 3.2f;
        [SerializeField] private float _holdDistance = 2.0f;
        [SerializeField] private float _minDistance = 1.2f;
        [SerializeField] private float _maxDistance = 3.2f;
        [SerializeField] private float _followSpeed = 14f;
        [SerializeField] private float _maxFollowSpeed = 9f;
        [SerializeField] private float _throwSpeed = 8f;

        private LayerMask _grabbableLayer; 

        private Camera _camera;
        private CharacterController _characterController;
        private Grabbable _currentHeldItem;
        private Rigidbody _currentHeldItemRb;
        private float _heldYaw;
        private Grabbable _targetedGrabbable;

        public bool IsCarrying => _currentHeldItem != null;
        public Grabbable TargetedGrabbable => _targetedGrabbable;

        void Start()
        {
            _camera = Game.Cam;
            _characterController = GetComponent<CharacterController>();
            _heldYaw = 0;

            _grabbableLayer = LayerMask.GetMask("Grabbable");
        }

        void FixedUpdate()
        {
            if (_currentHeldItem == null || _currentHeldItemRb == null) return;

            HandleHeldGrabbableMovement();
            HandleHeldGrabbableRotation();            
        }

        void Update()
        {
            TryTargetGrabbable();

            if (Game.State != GameState.Playing) return;

            HandleInputs();
        }

        private void HandleHeldGrabbableMovement()
        {
            Vector3 target = _camera.transform.position + _camera.transform.forward * _holdDistance;
            Vector3 distanceToTarget = target - _currentHeldItemRb.worldCenterOfMass;
            _currentHeldItemRb.linearVelocity = Vector3.ClampMagnitude(distanceToTarget * _followSpeed, _maxFollowSpeed);
        }

        private void HandleHeldGrabbableRotation()
        {
            Quaternion desiredRotation = Quaternion.Euler(0f, _camera.transform.eulerAngles.y + _heldYaw, 0f);
            _currentHeldItemRb.MoveRotation(Quaternion.Slerp(_currentHeldItemRb.rotation, desiredRotation, 12f * Time.fixedDeltaTime));
        }

        private void HandleInputs()
        {
            HandleGrabInput();
            HandleThrowInput();
            HandleZoomInput();
            HandleRotationInput();
        }

        private void HandleGrabInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_currentHeldItem != null)
                {
                    bool throwObject = false;
                    ReleaseGrabbableObject(throwObject);
                }
                else if (_targetedGrabbable != null)
                {
                    Grab(_targetedGrabbable);
                }
            }
        }

        private void HandleThrowInput()
        {
            if (_currentHeldItem == null) return;

            if (Input.GetMouseButtonDown(1))
            {
                bool throwObject = true;
                ReleaseGrabbableObject(throwObject);
            }
        }

        private void HandleZoomInput()
        {
            if (_currentHeldItem == null) return;

            float mouseScrollYInput = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(mouseScrollYInput) > 0.01f)
                _holdDistance = Mathf.Clamp(_holdDistance + Mathf.Sign(mouseScrollYInput) * 0.25f, _minDistance, _maxDistance);
        }

        private void HandleRotationInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _heldYaw += 90f;
            }
        }
                
        private void TryTargetGrabbable()
        {
            _targetedGrabbable = null;

            if (Game.State == GameState.Playing && _currentHeldItem == null)
                _targetedGrabbable = RaycastGrabbable();
        }

        Grabbable RaycastGrabbable()
        {
            RaycastHit hit;

            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
            
            if (Physics.Raycast(ray, out hit, _reach, _grabbableLayer, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.GetComponentInParent<Grabbable>();
            } 

            return null;
        }

        void Grab(Grabbable grabbable)
        {
            _currentHeldItem = grabbable;
            _currentHeldItemRb = grabbable.GetComponent<Rigidbody>();
            
            _heldYaw = 0f;
            if (_currentHeldItemRb != null)
            {
                _currentHeldItemRb.useGravity = false;
                _currentHeldItemRb.linearDamping = 8f;
                _currentHeldItemRb.angularDamping = 12f;
                _currentHeldItemRb.interpolation = RigidbodyInterpolation.Interpolate;
                _currentHeldItemRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            // Ignorar colision con el jugador: no se puede pararse sobre lo que se sostiene (anti-fly).
            SetIgnorePlayer(grabbable, true);
            grabbable.OnGrabbed(this);
            if (Game.Hud != null) Game.Hud.ShowCarry(true);
        }

        void ReleaseGrabbableObject(bool thrown)
        {
            if (_currentHeldItemRb != null)
            {
                _currentHeldItemRb.useGravity = true;
                _currentHeldItemRb.linearDamping = 0.2f;
                _currentHeldItemRb.angularDamping = 0.5f;
                
                if (thrown)
                    _currentHeldItemRb.linearVelocity = _camera.transform.forward * _throwSpeed;
                else
                    _currentHeldItemRb.linearVelocity = Vector3.ClampMagnitude(_currentHeldItemRb.linearVelocity, 2f);
            }

            Grabbable grabbable = _currentHeldItem;
            SetIgnorePlayer(grabbable, false);
            _currentHeldItem = null; 
            _currentHeldItemRb = null;

            if (grabbable != null) 
                grabbable.OnReleased();

            if (Game.Hud != null) 
                Game.Hud.ShowCarry(false);

            if (thrown && Game.Narrator != null)
                Game.Narrator.Event("throw");
        }

        void SetIgnorePlayer(Grabbable grabbable, bool ignore)
        {          
            if (ignore)
            {
                if (grabbable == null) return;
                Physics.IgnoreCollision(grabbable.GetComponent<Collider>(), _characterController, true);                                                 
            }
            else
            {
                Physics.IgnoreCollision(grabbable.GetComponent<Collider>(), _characterController, false);                
            }
        }

        /// <summary>Fuerza soltar (usado al resetear la sala).</summary>
        public void ForceDrop()
        {
            if (_currentHeldItem != null) 
                ReleaseGrabbableObject(false);
        }
    }
}
