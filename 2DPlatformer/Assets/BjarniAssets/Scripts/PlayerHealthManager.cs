using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;    public int maxHealth = 5;
    public int currentHealth;
    public bool isActive = true;
    public GameObject[] hearts;

    AudioManager audioManager;

    void Awake()
    {
        Instance = this;
        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        audioManager.PlaySFX(audioManager.playerHit);
        UpdateHearts();

         if (ScreenFlash.Instance != null)
        {
            ScreenFlash.Instance.Flash(); // Trigger screen flash!
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            audioManager.PlaySFX(audioManager.death);
            StartCoroutine(GameController.Instance.Respawn(2f));
            
            currentHealth = maxHealth;
            UpdateHearts();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }
    }
}
