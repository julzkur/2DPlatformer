using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5; // Maximum health
    private int currentHealth;

    public GameObject heartsContainer; // Assign the heart UI prefab (parent object)
    private Transform[] hearts; // Array to store heart children
    public AudioSource hitSound;

    void Start()
    {
        currentHealth = maxHealth;

        hearts = new Transform[heartsContainer.transform.childCount];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = heartsContainer.transform.GetChild(i);
        }

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        // Hide hearts based on current health
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].gameObject.SetActive(i < currentHealth);
        }
    }

    public void TakeDamage()
    {
        currentHealth -= 1;
        if (hitSound != null) hitSound.Play();
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Destroy(gameObject); // Remove player from the game
        // GameManager gm = FindObjectOfType<GameManager>();
        // if (gm != null) 
        // {
        //     gm.GameOver();
        // }
    }
}
