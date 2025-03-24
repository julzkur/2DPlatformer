using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    Rigidbody2D playerRb;
    Vector2 checkpointPos;

    // CameraController camController;

    void Awake()
    {
        checkpointPos = transform.position;
    }

    IEnumerator Respawn(float duration) 
    {
        playerRb.linearVelocity = Vector2.zero;
        playerRb.simulated = false;
        transform.localScale = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(duration);
        transform.position = checkpointPos;
        transform.localScale = new Vector3(1, 1, 1);;
        playerRb.simulated = true;
    }

    public void UpdateCheckpoint(Vector2 pos)
    {
        checkpointPos = pos;
        Debug.Log("Checkpoint updated");
    }

    void Update()
    {
        
    }
}
