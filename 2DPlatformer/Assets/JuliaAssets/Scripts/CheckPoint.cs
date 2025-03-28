using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    GameController gameController;

    void Awake()
    {
        gameController = GameController.Instance;
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
