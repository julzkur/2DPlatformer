
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    GameController gameController;

    void Start()
    {
        gameController = GameController.Instance;
        if (gameController == null)
        {
            Debug.LogError("GameController instance is null! Ensure it exists in the scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameController.UpdateCheckpoint(transform.position);
            Destroy(gameObject);
        }
    }
}
