using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace Subject626
{
    /// <summary>Punto de entrada: construye la sala, el jugador y la UI al dar Play.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        Light sun;
        List<Light> roomLights;

        void Awake()
        {
            Game.Reset();
            Game.Boot = this;
            Physics.queriesHitTriggers = true;

            BuildLighting();

            // Si la sala ya esta HORNEADA en la escena, se usa tal cual (editable a mano);
            // si no, se genera por codigo como antes.
            Room baked = Object.FindFirstObjectByType<Room>(FindObjectsInactive.Include);
            RoomBuildResult built = (baked != null) ? RoomBuilder.Discover(baked) : RoomBuilder.Build();
            Game.Room = built.room;
            roomLights = built.roomLights;

            BuildPlayer(built.room.entrancePos, built.room.entranceYaw);
            BuildUI(built);

            built.room.CaptureStarts();
            Game.SetState(GameState.Playing);
        }

        void BuildLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.30f, 0.27f);
            RenderSettings.fog = false;

            GameObject sunGo = new GameObject("Sun");
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.intensity = 0.7f;
            sun.color = new Color(1f, 0.96f, 0.9f);
            sun.transform.rotation = Quaternion.Euler(48f, 35f, 0f);
        }

        void BuildPlayer(Vector3 spawn, float yaw)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.radius = 0.3f; cc.height = 1.8f; cc.center = new Vector3(0f, 0.9f, 0f);
            cc.slopeLimit = 55f; cc.stepOffset = 0.4f;

            GameObject head = new GameObject("Head");
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);

            GameObject camGo = new GameObject("Camera");
            camGo.transform.SetParent(head.transform, false);
            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 80f;
            cam.nearClipPlane = 0.03f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();

            PlayerController ctrl = player.AddComponent<PlayerController>();
            ctrl.head = head.transform;
            player.AddComponent<PlayerInteractor>();
            PlayerCarry carry = player.AddComponent<PlayerCarry>();

            Game.Player = player.transform;
            Game.Cam = cam;
            Game.Controller = ctrl;
            Game.Carry = carry;
        }

        void BuildUI(RoomBuildResult built)
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            HUD hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Build();
            Game.Hud = hud;

            KeypadUI keypad = new GameObject("KeypadUI").AddComponent<KeypadUI>();
            keypad.Build();

            Narrator narrator = new GameObject("Narrator").AddComponent<Narrator>();
            narrator.Build();
            Game.Narrator = narrator;

            RevealController reveal = new GameObject("RevealController").AddComponent<RevealController>();
            reveal.Build(sun, roomLights, built.backstageRoot, built.backstageSpawn, built.backstageYaw);
            Game.Reveal = reveal;

            RoundManager rounds = new GameObject("RoundManager").AddComponent<RoundManager>();
            rounds.Build(built.exits);
            Game.Rounds = rounds;

            new GameObject("DebugPanel").AddComponent<DebugPanel>();
        }

        void Update()
        {
            Keyboard k = Keyboard.current;
            if (k == null) return;

            if (k.rKey.wasPressedThisFrame && Game.State == GameState.Escaped)
            {
                Restart();
                return;
            }
        }

        public void Restart()
        {
            Game.Reset();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
