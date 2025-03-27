using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    public float yOffset = 2f;

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = new Vector3(player.position.x, player.position.y + yOffset, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
