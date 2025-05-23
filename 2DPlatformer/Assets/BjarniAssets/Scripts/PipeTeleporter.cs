using UnityEngine;

public class PipeTeleporter : MonoBehaviour
{
    public PipeTeleporter destinationPipe;
    public Transform exitPoint;
    public float teleportDelay = 0.5f;
    public AudioClip pipeSound;

    private bool isTeleporting = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isTeleporting) return;

        if (other.CompareTag("Player") && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
        {
            StartCoroutine(TeleportPlayer(other.gameObject));
        }
    }

    private System.Collections.IEnumerator TeleportPlayer(GameObject player)
    {
        isTeleporting = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        PlaySound(pipeSound, player.transform.position);

        yield return StartCoroutine(ScalePlayer(player.transform, Vector3.one, Vector3.zero, 0.25f));

        yield return new WaitForSeconds(teleportDelay);

        player.transform.position = destinationPipe.exitPoint.position;

        PlaySound(destinationPipe.pipeSound, player.transform.position);

        yield return StartCoroutine(ScalePlayer(player.transform, Vector3.zero, Vector3.one, 0.25f));

        // Enable physics
        if (rb) rb.simulated = true;

        yield return new WaitForSeconds(0.25f);
        isTeleporting = false;
    }

    private System.Collections.IEnumerator ScalePlayer(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}
