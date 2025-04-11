using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    Vector2 checkpointPos;
    AudioManager audioManager;

    // CameraController camController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            checkpointPos = player.transform.position; // Set initial checkpoint
        }
        else
        {
            Debug.LogError("Player not found! Ensure your player has the correct tag.");
        }

        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
    }

    public IEnumerator Respawn(float duration) 
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Respawn failed: Player not found!");
            yield break;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();

        playerRb.linearVelocity = Vector2.zero;
        playerRb.simulated = false;

        if (playerSprite != null)
        {
            playerSprite.enabled = false;
        }

        yield return new WaitForSeconds(duration);

        player.transform.position = checkpointPos;

        playerSprite.enabled = true; 
        playerRb.simulated = true;

    }

    public void UpdateCheckpoint(Vector2 pos)
    {
        audioManager.PlaySFX(audioManager.checkpoint);
        checkpointPos = pos;
        Debug.Log("Checkpoint updated");
    }

}
