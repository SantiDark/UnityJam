using UnityEngine;

public class DialogueAudioPlayer : MonoBehaviour
{
    private static DialogueAudioPlayer _instance;
    public static DialogueAudioPlayer Instance => _instance;
    
    private AudioSource _audioSource;

    public bool AudioDialogueIsPlaying => _audioSource.isPlaying;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayDialogue(AudioClip _dialogueAudio)
    {
        StopDialogue();
        _audioSource.PlayOneShot(_dialogueAudio);
    }

    public void StopDialogue()
    {
        _audioSource.Stop();
    }

}
