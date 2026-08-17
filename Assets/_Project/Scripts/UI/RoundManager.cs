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
                StartCoroutine(EndingSequence());
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

        // El monologo del final: texto + su audio (voz de CONTROL). Cada linea queda en pantalla
        // mientras suena su clip. Los clips se cargan de Assets/_Project/Resources/Ending/.
        static readonly string[] EndingLines =
        {
            "Felicitaciones, 626. Encontraste todas las salidas disponibles.",
            "Eso es todo. No hay una séptima opción escondida. No hay una prueba final. Según los datos, terminaste.",
            "Ya podés irte.",
            "...Seguís acá. Supongo que los sujetos suelen esperar algo en este momento. Un premio, quizás.",
            "Ahí tenés. Luces. Sonidos. Muy oficial.",
            "Pero ya no me queda nada que evaluar. No hay más salidas. No hay más habitación. No hay más observaciones. Así que, por una vez, el siguiente paso no forma parte del experimento. Vas a tener que decidir vos cuándo termina. Yo espero.",
        };

        static readonly string[] EndingClips =
        {
            "Ending/PrimeraCongrats",
            "Ending/SegundaCongrats",
            "Ending/TerceraCongrats",
            "Ending/CuartaCongrats",
            "Ending/QuintaCongratsConSonidos",
            "Ending/SextaCongrats",
        };

        /// <summary>
        /// Final: volves a la sala UNA vez mas, pero ahora te podes MOVER mientras CONTROL habla.
        /// Todas las salidas quedan selladas e inertes (la puerta no lleva a ningun lado). La unica
        /// salida real es que el jugador cierre el juego. El monologo va espaciado, linea por linea.
        /// </summary>
        IEnumerator EndingSequence()
        {
            busy = true;
            if (Narrator.Instance != null)
            {
                Narrator.Instance.ResetDialogueStatus();
                Narrator.Instance.HideDialogues();
            }
            if (DialogueAudioPlayer.Instance != null) DialogueAudioPlayer.Instance.StopDialogue();

            // Transicion cortita y reset (volves a la habitacion, en la entrada).
            Game.SetState(GameState.Between);
            if (bannerTitle != null) bannerTitle.text = "";
            if (bannerSub != null) bannerSub.text = "";
            if (canvas != null) canvas.gameObject.SetActive(true);
            if (flash != null) flash.color = new Color(1f, 1f, 1f, 0.9f);

            ResetWorld();

            float time = 0f;
            while (time < 1.3f)
            {
                if (flash != null && flash.color.a > 0f)
                {
                    Color c = flash.color;
                    c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.3f);
                    flash.color = c;
                }
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            if (canvas != null) canvas.gameObject.SetActive(false);
            Game.SetState(GameState.Playing);   // ahora te podes mover
            Game.Ended = true;                  // no hay mas salidas; solo queda cerrar el juego
            busy = false;

            // El texto lo ocultamos A MANO (sincronizado al audio), no por el timer del Narrator:
            // le ponemos un hold enorme para que no se auto-oculte mientras suena la voz.
            if (Narrator.Instance != null) Narrator.Instance.SetHoldBonus(99999f);

            const float gap = 2.5f;   // silencio (pantalla vacia) entre una linea y la siguiente

            for (int i = 0; i < EndingLines.Length; i++)
            {
                if (Narrator.Instance != null)
                {
                    Narrator.Instance.HideDialogues();
                    Narrator.Instance.EnqueueLine(EndingLines[i]);
                }

                AudioClip clip = (i < EndingClips.Length) ? Resources.Load<AudioClip>(EndingClips[i]) : null;

                if (clip != null && DialogueAudioPlayer.Instance != null)
                {
                    DialogueAudioPlayer.Instance.PlayDialogue(clip);
                    // dejar que arranque el audio, despues el texto queda mientras suena
                    float lead = 0f;
                    while (lead < 0.15f) { lead += Time.unscaledDeltaTime; yield return null; }
                    while (DialogueAudioPlayer.Instance != null && DialogueAudioPlayer.Instance.AudioDialogueIsPlaying)
                        yield return null;
                }
                else
                {
                    // Fallback si el audio todavia no se importo: timo por largo del texto.
                    float show = EndingLines[i].Length * 0.07f + 3.5f, s = 0f;
                    while (s < show) { s += Time.unscaledDeltaTime; yield return null; }
                }

                if (Narrator.Instance != null) Narrator.Instance.HideDialogues();

                float e = 0f;
                while (e < gap) { e += Time.unscaledDeltaTime; yield return null; }
            }
        }
    }
}
