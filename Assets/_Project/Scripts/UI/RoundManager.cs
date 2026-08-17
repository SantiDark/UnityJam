using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    /// <summary>
    /// El corazon del juego: escapar NO termina, es una ronda del test. Cada salida usada se
    /// registra y se SELLA para las siguientes rondas (te obliga a buscar otra). Ganas de verdad
    /// al encontrar las SEIS. Maneja el contador, el banner con la provocacion y el reset.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        private static RoundManager _instance;
        public static RoundManager Instance => _instance;

        private Dictionary<string, AudioClip> _endingAudios = new Dictionary<string, AudioClip>();

        readonly bool[] discovered = new bool[ExitInfo.Count];
        readonly List<IExit> exits = new List<IExit>();
        int found;

        Canvas canvas;
        Text bannerTitle;
        Text bannerSub;
        Image flash;
        bool busy;

        private bool _hasFoundFirstExit;
        public bool HasFoundFirstExit => _hasFoundFirstExit;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            _hasFoundFirstExit = false;
        }

        public void Build(List<IExit> exits, List<DialogueAudio> dialogueAudios)
        {
            _endingAudios.Clear();

            foreach(DialogueAudio dialogueAudio in dialogueAudios)
            {
               _endingAudios.Add(dialogueAudio.DialogueType, dialogueAudio.Audio);
            }

            this.exits.Clear();
            if (exits != null) this.exits.AddRange(exits);

            canvas = UIFactory.Canvas("Round_Canvas", 45);
            UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.78f),
                new Vector2(3000f, 260f), Vector2.zero, new Vector2(0.5f, 0.5f));
            bannerTitle = UIFactory.Label(canvas.transform, "", new Vector2(1700f, 70f), new Vector2(0f, 45f),
                new Vector2(0.5f, 0.5f), 44, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.DevOrange);
            bannerSub = UIFactory.Label(canvas.transform, "", new Vector2(1700f, 50f), new Vector2(0f, -35f),
                new Vector2(0.5f, 0.5f), 26, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.9f));
            flash = UIFactory.Stretch(canvas.transform, new Color(1f, 1f, 1f, 0f));
            flash.raycastTarget = false;
            canvas.gameObject.SetActive(false);

            if (Game.Hud != null) Game.Hud.SetFound(0, ExitInfo.Count);
        }

        public int Found { get { return found; } }
        public bool IsDiscovered(ExitId id) { return discovered[(int)id]; }

        /// <summary>Se llamo una salida. Registra, y si faltan, sella + sigue; si estan todas, final.</summary>
        public void Escape(ExitId id)
        {
            if (busy) return;
            if (discovered[(int)id]) return;   // ya contada (guarda anti-doble)

            discovered[(int)id] = true;
            found++;
            if (Game.Hud != null) Game.Hud.SetFound(found, ExitInfo.Count);

            if (found >= ExitInfo.Count)
            {
                if (Game.Reveal != null) Game.Reveal.FinalReveal();
                return;
            }
            StartCoroutine(RoundClear(id));
        }

        /// <summary>Reset sin contar (troll de la puerta / recomenzar la ronda).</summary>
        public void SoftReset()
        {
            ResetWorld();
        }

        void ResetWorld()
        {
            if (Game.Carry != null) Game.Carry.ForceDrop();
            if (Game.Room != null) Game.Room.ResetProps();
            foreach (IExit e in exits)
                if (e != null) e.ResetForRound(discovered[(int)e.Id]);
            if (Game.Room != null) Game.Room.RespawnPlayer();
        }

        IEnumerator RoundClear(ExitId id)
        {
            Narrator.Instance.ResetDialogueStatus();
            Narrator.Instance.HideDialogues();
            _hasFoundFirstExit = true;

            busy = true;
            Game.SetState(GameState.Between);

            if (bannerTitle != null)
                bannerTitle.text = "Salida registrada: " + ExitInfo.Name(id);

            if (bannerSub != null)
            {
                string exitText = ExitInfo.Text(id);                
                bannerSub.text = exitText;

                DialogueAudioPlayer.Instance.PlayDialogue(_endingAudios[id.ToString()]);
            }

            if (canvas != null) canvas.gameObject.SetActive(true);
            if (flash != null) flash.color = new Color(1f, 1f, 1f, 0.9f);

            while (DialogueAudioPlayer.Instance.AudioDialogueIsPlaying)
            {
                if (flash != null && flash.color.a > 0f)
                {
                    Color c = flash.color;
                    c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.4f);
                    flash.color = c;
                }
                //time += Time.unscaledDeltaTime;
                yield return null;
            }

            float time = 0f;

            while(time < 2f)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            ResetWorld();
            if (canvas != null) canvas.gameObject.SetActive(false);
            Game.SetState(GameState.Playing);
            busy = false;
        }                
    }
}
