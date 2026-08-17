using UnityEngine;
using UnityEngine.InputSystem;

namespace Subject626
{
    /// <summary>Movimiento en primera persona con CharacterController: caminar, correr, saltar, mirar.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _instance;
        public static PlayerController Instance => _instance;

        [Header("Velocidades")]
        public float walkSpeed = 3.4f;
        public float sprintSpeed = 5.6f;
        public float jumpSpeed = 5.2f;
        public float gravity = -17f;
        public float mouseSensitivity = 0.12f;

        public Transform head;
        public float CurrentSpeed { get; private set; }
        public bool Grounded { get; private set; }

        CharacterController cc;
        float pitch;
        float yaw;
        float vy;
        float bobT;
        Vector3 headBase;

        private bool _isMoving;
        public bool IsMoving => _isMoving;

        private bool _isMovingMouse;
        public bool IsMovingMouse => _isMovingMouse;

        void Awake()
        {
            _instance = this;
            cc = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
        }

        void Start()
        {
            if (head != null) headBase = head.localPosition;
        }

        void Update()
        {
            bool active = (Game.State == GameState.Playing);
            if (active) Look();
            Move(active);
        }

        void Look()
        {
            if (Mouse.current == null) return;
            Vector2 d = Mouse.current.delta.ReadValue();

            _isMovingMouse = d.x != 0 || d.y != 0;

            yaw += d.x * mouseSensitivity;
            pitch -= d.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -88f, 88f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (head != null) head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move(bool active)
        {
            Keyboard k = Keyboard.current;
            Vector2 input = Vector2.zero;
            bool wantJump = false;
            bool wantSprint = false;

            if (active && k != null)
            {
                if (k.wKey.isPressed) input.y += 1f;
                if (k.sKey.isPressed) input.y -= 1f;
                if (k.dKey.isPressed) input.x += 1f;
                if (k.aKey.isPressed) input.x -= 1f;
                wantJump = k.spaceKey.wasPressedThisFrame;
                wantSprint = k.leftShiftKey.isPressed;
            }
            input = Vector2.ClampMagnitude(input, 1f);

            _isMoving = input.x != 0 || input.y != 0;

            float speed = wantSprint ? sprintSpeed : walkSpeed;
            bool moving = input.sqrMagnitude > 0.01f;
            CurrentSpeed = moving ? speed : 0f;

            Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed;

            Grounded = cc.isGrounded;
            if (Grounded)
            {
                if (vy < 0f) vy = -2f;
                if (wantJump) vy = jumpSpeed;
            }
            vy += gravity * Time.deltaTime;
            move.y = vy;

            cc.Move(move * Time.deltaTime);
            HeadBob(moving && Grounded, speed);
        }

        void HeadBob(bool moving, float speed)
        {
            if (head == null) return;
            Vector3 target = headBase;
            if (moving)
            {
                bobT += Time.deltaTime * speed * 1.7f;
                target.y = headBase.y + Mathf.Sin(bobT) * 0.05f;
                target.x = headBase.x + Mathf.Cos(bobT * 0.5f) * 0.035f;
            }
            head.localPosition = Vector3.Lerp(head.localPosition, target, 10f * Time.deltaTime);
        }

        public void Teleport(Vector3 pos, float lookYaw)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
            vy = 0f;
            yaw = lookYaw;
            pitch = 0f;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (head != null) head.localRotation = Quaternion.identity;
        }
    }
}
