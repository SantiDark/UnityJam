using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Subject626
{
    
    public class Narrator : MonoBehaviour
    {
        private Image _dialoguePanel;
        private Text _speakerNameTextDisplayer;
        private Text _dialogueTextDisplayer;

        private readonly Queue<string> _pendingLines = new Queue<string>();
        
        static readonly HashSet<string> _onceEvents = new HashSet<string> { "grab", "throw", "elevated" };

        private readonly HashSet<string> _triggeredOnceEvents = new HashSet<string>();

        private string _currentDisplayedDialogue;
        private float _dialogueClearTime;
        
        private float _nextIdleDialogueTime;
        private int _idleDialogueIndex;

        private void Start()
        {
            foreach (string sentence in Welcome)
            {
                _pendingLines.Enqueue(sentence);
            }
        }

        private void Update()
        {
            float currentGameTime = Time.unscaledTime;
                        
            HandleDialogues(currentGameTime);            
        }

        private void HandleDialogues(float currentGameTime)
        {
            if (_currentDisplayedDialogue == null && _pendingLines.Count > 0)
            {
                DisplayDialogues(currentGameTime);
            }
            else if (_currentDisplayedDialogue != null && currentGameTime >= _dialogueClearTime)
            {
                HideDialogues(currentGameTime);
            }

            HandleJumpDialogue();
            HandleIdleDialogues(currentGameTime);            
        }

        private void DisplayDialogues(float currentGameTime)
        {
            _currentDisplayedDialogue = _pendingLines.Dequeue();

            if (_dialogueTextDisplayer != null)
                _dialogueTextDisplayer.text = _currentDisplayedDialogue;

            if (_dialoguePanel != null)
                _dialoguePanel.gameObject.SetActive(true);

            _dialogueClearTime = currentGameTime + Mathf.Clamp(_currentDisplayedDialogue.Length * 0.055f, 2.5f, 5.5f);
        }

        private void HideDialogues(float currentGameTime)
        {
            _currentDisplayedDialogue = null;

            if (_dialoguePanel != null)
                _dialoguePanel.gameObject.SetActive(false);

            _nextIdleDialogueTime = currentGameTime + Random.Range(24f, 40f);
        }

        private void HandleJumpDialogue()
        {
            if (Game.State == GameState.Playing && Game.Player != null && Game.Player.position.y > 1.3f)
                TryEnqueueDialogue("elevated");
        }

        private void HandleIdleDialogues(float currentGameTime)
        {
            if (_currentDisplayedDialogue == null && _pendingLines.Count == 0 && Game.State == GameState.Playing
                && currentGameTime >= _nextIdleDialogueTime)
            {
                _pendingLines.Enqueue(IdleTexts[_idleDialogueIndex % IdleTexts.Length]);
                _idleDialogueIndex++;
                _nextIdleDialogueTime = currentGameTime + Random.Range(28f, 46f);
            }
        }


        private static readonly string[] Welcome =
        {
            "Bienvenido, Experimento 626.",
            "Tu tarea es simple: encontrar la forma de salir de esta habitación.",
            "Hay varias salidas, aunque cómo las encuentres —y en qué orden— depende completamente de vos.",
            "Técnicamente, la puerta siempre está abierta.", 
            "Pero simplemente atravesarla arruinaría un poco el propósito, ¿no?",
            "Sé creativo. Sorprendeme."
        };

        private static readonly string[] IdleTexts =
        {
            "Los sensores dicen que sigue adentro. Curioso.",
            "Tomese su tiempo. Nosotros tenemos de sobra.",
            "Cada objeto de esta sala esta ahi por una razon.",
            "La mayoria de los sujetos ya habria probado algo.",
            "Rendirse tambien es un dato util, sabe.",
            "La salida no siempre es la mas obvia.",
            "No toca lo que esperabamos que tocara. Anotado.",
            "Respire. Piense. Y despues rompa algo, si hace falta.",
        };

        private static string[] GetDialogueFor(string eventType)
        {
            switch (eventType)
            {
                case "grab": 
                    return new[] 
                    { "¡Perfecto! Ya empezaste mejor que la mayoría. Prometedor." };

                case "throw": 
                    return new[] 
                    { "Agresivo. Eso tambien lo anotamos." };

                case "elevated": 
                    return new[] 
                    { "Sube. Veamos hasta donde llega." };

                case "pit":
                    return new[] 
                    { "Ups. De vuelta al principio. Sin rencores.",
                      "La gravedad tambien es parte del ensayo." };

                case "door_troll":
                    return new[] 
                    { "Sí, sí, se abrió. Felicitaciones. Lamentablemente, eso no cuenta para tu progreso. " +  
                    "\n[Fuera de escena] ¡Ey! El 626 no es particularmente creativo, ¿no?" };
                
                case "key":
                    return new[] 
                    { "Una llave debajo de la alfombra. Un clásico. " + 
                      "\nTe sorprendería saber cuánta gente nunca piensa en mirar hacia abajo." };
                
                case "sealed":
                    return new[] 
                    { "Esa salida ya la conoce. La cerramos. Busque otra.",
                      "Repetir no cuenta, sujeto 626." };
            }
            return null;
        }

        public void TryEnqueueDialogue(string eventType)
        {
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
        }

        private void ResetDialogueStatus()
        {
            _currentDisplayedDialogue = null;
            _dialogueClearTime = 0f;
            _pendingLines.Clear();
            _nextIdleDialogueTime = Time.unscaledTime + 18f;
        }

        public void Build()
        {
            Canvas canvas = UIFactory.Canvas("Narrator_Canvas", 25);

            _dialoguePanel = UIFactory.Panel(canvas.transform, new Color(0f, 0f, 0f, 0.55f),
                new Vector2(1300f, 96f), new Vector2(0f, 190f), new Vector2(0.5f, 0f));

            _speakerNameTextDisplayer = UIFactory.Label(_dialoguePanel.transform, "CONTROL 626", new Vector2(1240f, 24f), new Vector2(0f, 30f),
                new Vector2(0.5f, 0.5f), 18, FontStyle.Bold, TextAnchor.MiddleCenter, MaterialLib.DevOrange);

            _dialogueTextDisplayer = UIFactory.Label(_dialoguePanel.transform, "", new Vector2(1240f, 56f), new Vector2(0f, -8f),
                new Vector2(0.5f, 0.5f), 24, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.92f, 0.92f, 0.95f));

            _dialoguePanel.gameObject.SetActive(false);
            _nextIdleDialogueTime = 12f;
        }



    }
}
