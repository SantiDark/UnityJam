using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/DialogueAudio")]
public class DialogueAudio : ScriptableObject
{
    [SerializeField] private string _dialogueType;
    public string DialogueType => _dialogueType;

    [SerializeField] private AudioClip _audio;
    public AudioClip Audio => _audio;
}
