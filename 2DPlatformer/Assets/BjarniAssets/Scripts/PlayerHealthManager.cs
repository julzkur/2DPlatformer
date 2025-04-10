using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;    public int maxHealth = 5;
    public int currentHealth;
    public bool isActive = true;
    public GameObject[] hearts;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHearts();

         if (ScreenFlash.Instance != null)
        {
            ScreenFlash.Instance.Flash(); // Trigger screen flash!
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
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
