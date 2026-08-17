using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    
    public class Narrator : MonoBehaviour
    {
        private static int _fallCounter;

        private static Narrator _instance;
        public static Narrator Instance => _instance;

        private static Dictionary<string, DialogueAudio> _dialogueAudios = new Dictionary<string, DialogueAudio>(); 

        private Image _dialoguePanel;
        private Text _speakerNameTextDisplayer;
        private Text _dialogueTextDisplayer;

        private readonly Queue<string> _pendingLines = new Queue<string>();
        
        static readonly HashSet<string> _onceEvents = new HashSet<string> { "grab", "throw", "elevated" };

        private readonly HashSet<string> _triggeredOnceEvents = new HashSet<string>();

        private string _currentDisplayedDialogue;
        private float _dialogueClearTime;
        
        private float _dialogueTimer;
        private float _idleTimer;
        private bool _idleDialogueShown;

        private float _firstExitTimer;
        private bool _exitDialogueShown;

        private bool _keypadDialogueShown;

        private float _keyTimer;
        private bool _keyDialogueShown;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            _idleTimer = 0;
            _fallCounter = 0;
            _firstExitTimer = 0;

            _idleDialogueShown = false;
            _exitDialogueShown = false;
            _keypadDialogueShown = false;
            _keyDialogueShown = false;

            foreach (string sentence in Welcome)
            {
                _pendingLines.Enqueue(sentence);
            }

            AudioClip clip = _dialogueAudios["intro"].Audio;
            DialogueAudioPlayer.Instance.PlayDialogue(clip);
        }

        private void Update()
        {
            _dialogueTimer = Time.unscaledTime;
                        
            HandleDialogues();            
            
        }

        private void HandleDialogues()
        {
            if (_currentDisplayedDialogue == null && _pendingLines.Count > 0)
            {
                DisplayDialogues();
            }
            else if (_currentDisplayedDialogue != null && _dialogueTimer >= _dialogueClearTime)
            {
                HideDialogues();
            }

            HandleIdleDialogue();
            HandleNoExitFoundDialogue();
            HandleKeypadDialogue();
            HandleKeyDialogue();
        }

        private void DisplayDialogues()
        {
            _currentDisplayedDialogue = _pendingLines.Dequeue();

            if (_dialogueTextDisplayer != null)
                _dialogueTextDisplayer.text = _currentDisplayedDialogue;

            if (_dialoguePanel != null)
                _dialoguePanel.gameObject.SetActive(true);

            _dialogueClearTime = _dialogueTimer + (_currentDisplayedDialogue.Length * 0.07f);
        }

        public void HideDialogues()
        {
            _currentDisplayedDialogue = null;

            if (_dialoguePanel != null)
                _dialoguePanel.gameObject.SetActive(false);

        }               

        private void HandleIdleDialogue()
        {
            if (_currentDisplayedDialogue == null && _pendingLines.Count == 0 && Game.State == GameState.Playing)
            {
                if(!PlayerController.Instance.IsMoving && !PlayerController.Instance.IsMovingMouse)
                {
                    _idleTimer += Time.deltaTime;

                    if (_idleTimer >= 60 && !_idleDialogueShown)
                    {
                        ResetDialogueStatus();

                        _pendingLines.Enqueue("Bueno... parece que el Sujeto 626 está quemándose el cerebro mirando TikTok o Reels.\n                               Con este, ya van 589 sujetos que hicieron lo mismo.                                        ");
                        DialogueAudioPlayer.Instance.PlayDialogue(_dialogueAudios["idle"].Audio);
                        _idleDialogueShown = true;
                    }
                }
                else
                {
                    _idleTimer = 0;
                    _idleDialogueShown = false;
                }
                                
            }
            else
            {
                _idleTimer = 0;
                _idleDialogueShown = false;
            }
        }

        private void HandleNoExitFoundDialogue()
        {
            if (RoundManager.Instance.HasFoundFirstExit || _exitDialogueShown) return;

            _firstExitTimer += Time.deltaTime;

            if(_firstExitTimer >= 180)
            {
                ResetDialogueStatus();

                _pendingLines.Enqueue("Tres minutos y todavía no encontraste ni una salida. Uno pensaría que eso es un problema... pero en realidad, son datos interesantes. " +
                    "Actualmente lo estás haciendo peor que el 98% de los participantes.");

                DialogueAudioPlayer.Instance.PlayDialogue(_dialogueAudios["exit"].Audio);

                _exitDialogueShown = true;
            }
        }

        private void HandleKeypadDialogue()
        {
            if (_keypadDialogueShown) return;

            if(KeypadUI.Instance.FiveMatchingAttempts)
            {
                ResetDialogueStatus();

                _pendingLines.Enqueue("Bueno... el Sujeto 626 revisó el mismo número cinco veces. Posibles problemas de memoria.\n" +
                    "                          Anotado.                         ");
                DialogueAudioPlayer.Instance.PlayDialogue(_dialogueAudios["attempts"].Audio);

                _keypadDialogueShown = true;
            }
        }

        private void HandleKeyDialogue()
        {
            if(_keyDialogueShown) return;

            if(Game.HasKey)
            {
                _keyTimer += Time.deltaTime;    

                if(_keyTimer >= 90)
                {
                    ResetDialogueStatus();

                    _pendingLines.Enqueue("Tenés una llave en la mano, 626. No te voy a decir que hacer con eso. Simplemente estoy haciendo una observación.");
                    DialogueAudioPlayer.Instance.PlayDialogue(_dialogueAudios["key"].Audio);

                    _keyDialogueShown = true;
                }
            }
            else
            {
                _keyTimer = 0;
            }
        }


        private static readonly string[] Welcome =
        {
            "                   Bienvenido, Experimento 626.                   ",
            "Tu tarea es simple: encontrar la forma de salir de esta habitación.",
            "        Hay varias salidas, aunque cómo las encuentres —y en qué orden— depende completamente de vos.       ",
            "  Técnicamente, la puerta siempre está abierta.  ", 
            "Pero simplemente atravesarla arruinaría un poco el propósito, ¿no?",             
            "Sé creativo. Sorprendeme."
        };
       

        public void TryEnqueueDialogue(string eventType)
        {
            if(eventType == "pit")
            {
                _fallCounter++;

                if(_fallCounter != 2)
                {
                    HideDialogues();
                    ResetDialogueStatus();
                    DialogueAudioPlayer.Instance.StopDialogue();
                    return;
                }
            }

            string[] dialogueOptions = GetDialogueFor(eventType);

            if (dialogueOptions == null || dialogueOptions.Length == 0) return;

            if (_onceEvents.Contains(eventType))
            {
                if (_triggeredOnceEvents.Contains(eventType)) return;

                _triggeredOnceEvents.Add(eventType);
            }

            string randomDialogue = dialogueOptions[Random.Range(0, dialogueOptions.Length)];

            ResetDialogueStatus();

            _pendingLines.Enqueue(randomDialogue);

            AudioClip clip = _dialogueAudios[eventType].Audio;
            DialogueAudioPlayer.Instance.PlayDialogue(clip);
        }

        public void ResetDialogueStatus()
        {
            _currentDisplayedDialogue = null;
            _dialogueClearTime = 0f;
            _pendingLines.Clear();
        }

        private static string[] GetDialogueFor(string eventType)
        {           
            switch (eventType)
            {               
                case "grab":                                     
                    return new[] 
                    { "           ¡Perfecto! Ya empezaste mejor que la mayoría. Prometedor.           " };
                                
                case "pit":                               
                    return new[]
                    { "                  Ya van dos, 626. La plataforma no se movió. Lo comprobé.                  " };
                  
                case "door_troll":
                    return new[] 
                    { "                                 Sí, sí, se abrió. Felicitaciones. Lamentablemente, eso no cuenta para tu progreso.                                  " +  
                    "\n[Fuera de escena] ¡Ey! El 626 no es particularmente creativo, ¿no?"};
                                
                case "sealed":
                    return new[]
                    { "                                 Sí, sí, se abrió. Felicitaciones. Lamentablemente, eso no cuenta para tu progreso.                                  " +
                    "\n[Fuera de escena] ¡Ey! El 626 no es particularmente creativo, ¿no?"};
            }
            return null;
        }

        

        public void Build(List<DialogueAudio> dialogues)
        {
            _dialogueAudios.Clear();

            foreach (DialogueAudio dialogueAudio in dialogues)
            {
                _dialogueAudios.Add(dialogueAudio.DialogueType, dialogueAudio);
            }                        

            Canvas canvas = UIFactory.Canvas("Narrator_Canvas", 25);

            _dialoguePanel = UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.55f),
                new Vector2(1300f, 96f), new Vector2(0f, 190f), new Vector2(0.5f, 0f));

            _speakerNameTextDisplayer = UIFactory.Label(_dialoguePanel.transform, "CONTROL 626", new Vector2(1240f, 24f), new Vector2(0f, 30f),
                new Vector2(0.5f, 0.5f), 18, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.DevOrange);

            _dialogueTextDisplayer = UIFactory.Label(_dialoguePanel.transform, "", new Vector2(1240f, 56f), new Vector2(0f, -8f),
                new Vector2(0.5f, 0.5f), 24, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.95f));

            _dialoguePanel.gameObject.SetActive(false);
        }              

    }
}
