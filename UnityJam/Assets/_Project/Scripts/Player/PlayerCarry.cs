using System.Collections.Generic;
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
        public float reach = 3.2f;
        public float holdDist = 2.0f;
        public float minDist = 1.2f;
        public float maxDist = 3.2f;
        public float followForce = 14f;
        public float throwSpeed = 8f;

        Camera cam;
        CharacterController playerCol;
        Grabbable held;
        Rigidbody heldRb;
        float heldYaw;
        Grabbable hovered;
        readonly List<Collider> ignored = new List<Collider>();

        public bool IsCarrying { get { return held != null; } }
        public Grabbable Hovered { get { return hovered; } }

        void Start()
        {
            cam = Game.Cam;
            playerCol = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (cam == null) cam = Game.Cam;
            if (cam == null) return;

            hovered = null;
            if (Game.State == GameState.Playing && held == null)
                hovered = RaycastGrabbable();

            Mouse m = Mouse.current;
            Keyboard k = Keyboard.current;
            if (Game.State != GameState.Playing) return;
            if (m == null) return;

            if (m.leftButton.wasPressedThisFrame)
            {
                if (held != null) Release(false);
                else if (hovered != null) Grab(hovered);
            }
            if (m.rightButton.wasPressedThisFrame && held != null)
                Release(true);

            if (held != null)
            {
                float scroll = m.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    holdDist = Mathf.Clamp(holdDist + Mathf.Sign(scroll) * 0.25f, minDist, maxDist);
                if (k != null && k.rKey.wasPressedThisFrame)
                    heldYaw += 90f;
            }
        }

        void FixedUpdate()
        {
            if (held == null || heldRb == null) return;

            Vector3 target = cam.transform.position + cam.transform.forward * holdDist;
            Vector3 toTarget = target - heldRb.worldCenterOfMass;
            heldRb.linearVelocity = Vector3.ClampMagnitude(toTarget * followForce, 9f);

            Quaternion desired = Quaternion.Euler(0f, cam.transform.eulerAngles.y + heldYaw, 0f);
            heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, desired, 12f * Time.fixedDeltaTime));
        }

        Grabbable RaycastGrabbable()
        {
            RaycastHit hit;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out hit, reach, ~0, QueryTriggerInteraction.Ignore))
                return hit.collider.GetComponentInParent<Grabbable>();
            return null;
        }

        void Grab(Grabbable g)
        {
            held = g;
            heldRb = g.GetComponent<Rigidbody>();
            heldYaw = 0f;
            if (heldRb != null)
            {
                heldRb.useGravity = false;
                heldRb.linearDamping = 8f;
                heldRb.angularDamping = 12f;
                heldRb.interpolation = RigidbodyInterpolation.Interpolate;
                heldRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            // Ignorar colision con el jugador: no se puede pararse sobre lo que se sostiene (anti-fly).
            SetIgnorePlayer(g, true);
            g.OnGrabbed(this);
            if (Game.Hud != null) Game.Hud.ShowCarry(true);
        }

        void Release(bool thrown)
        {
            if (heldRb != null)
            {
                heldRb.useGravity = true;
                heldRb.linearDamping = 0.2f;
                heldRb.angularDamping = 0.5f;
                if (thrown)
                    heldRb.linearVelocity = cam.transform.forward * throwSpeed;
                else
                    heldRb.linearVelocity = Vector3.ClampMagnitude(heldRb.linearVelocity, 2f);
            }
            Grabbable g = held;
            SetIgnorePlayer(g, false);
            held = null; heldRb = null;
            if (g != null) g.OnReleased();
            if (Game.Hud != null) Game.Hud.ShowCarry(false);
            if (thrown && Game.Narrator != null) Game.Narrator.Event("throw");
        }

        void SetIgnorePlayer(Grabbable g, bool ignore)
        {
            if (playerCol == null) playerCol = GetComponent<CharacterController>();
            if (playerCol == null) return;
            if (ignore)
            {
                ignored.Clear();
                if (g == null) return;
                g.GetComponentsInChildren<Collider>(true, ignored);
                foreach (Collider c in ignored)
                    if (c != null && !c.isTrigger) Physics.IgnoreCollision(c, playerCol, true);
            }
            else
            {
                foreach (Collider c in ignored)
                    if (c != null && !c.isTrigger) Physics.IgnoreCollision(c, playerCol, false);
                ignored.Clear();
            }
        }

        /// <summary>Fuerza soltar (usado al resetear la sala).</summary>
        public void ForceDrop()
        {
            if (held != null) Release(false);
        }
    }
}
