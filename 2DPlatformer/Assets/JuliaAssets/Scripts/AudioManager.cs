using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip background;
    public AudioClip jump;
    public AudioClip land;
    public AudioClip playerShoot;
    public AudioClip enemyShoot; 
    public AudioClip playerHit;
    public AudioClip enemyHit;
    public AudioClip checkpoint; 
    public AudioClip lever;
    public AudioClip death;
    public AudioClip respawn;
    public AudioClip win;
    public AudioClip pickup;
    public AudioClip enemyDeath;
    public AudioClip pipeIn;
    public AudioClip pipeOut;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.volume = 0.2f;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

}
