using UnityEngine;

public class ToolProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    public float rotationSpeed = 360f;
    public float gravityScale = 1f;

    private Rigidbody2D rb;
    private Vector2 throwDirection;
    private float faceDirection = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * -faceDirection); 
        rb.angularVelocity = rotationSpeed * -faceDirection;
    }

    public void SetDirectionandForce(float holdTime, float throwForce, float maxThrowDistance, float dir)
    {
        faceDirection = dir;

        // Flip sprite based on direction
        if (dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        throwDirection = new Vector2(dir, 1).normalized;

        float force = Mathf.Min(holdTime * throwForce, maxThrowDistance);

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.linearVelocity = throwDirection * force;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            return;
        }
        Destroy(gameObject);
    }
}
