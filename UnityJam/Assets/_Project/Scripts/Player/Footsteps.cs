using Subject626;
using UnityEngine;

public class Footsteps : MonoBehaviour
{    
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        PlayerController player = PlayerController.Instance;
        if (Game.State != GameState.Playing || player == null || _audioSource == null)
        {
            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.Stop();
            return;
        }

        if (player.IsMoving)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        else
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }
    }
}
