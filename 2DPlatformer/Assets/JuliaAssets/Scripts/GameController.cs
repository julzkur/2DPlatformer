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
        Instance = this;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        checkpointPos = player.transform.position;
    }

    public IEnumerator Respawn(float duration) 
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();

        playerRb.linearVelocity = Vector2.zero;
        playerRb.simulated = false;
        sprite.enabled = false; // Hide player

        yield return new WaitForSeconds(duration);

        player.transform.position = checkpointPos;
        playerRb.simulated = true;

        // transform.localScale = new Vector3(0, 0, 0);
        //yield return new WaitForSeconds(duration);
        //transform.position = checkpointPos;
        //transform.localScale = new Vector3(1, 1, 1);;
        //playerRb.simulated = true;
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
