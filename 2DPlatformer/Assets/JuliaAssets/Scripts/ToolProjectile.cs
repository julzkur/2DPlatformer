using UnityEngine;

public class ToolProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    public float rotationSpeed = 360f;
    public float gravityScale = 1f;

    // Sprite Cycling
    [Header("Visuals")]
    public Sprite[] projectileSprites = new Sprite[3]; // Assign 3 sprites in Inspector
    private SpriteRenderer spriteRenderer;
    private static int nextSpriteIndex = 0; 

    private Rigidbody2D rb;
    private Vector2 throwDirection;
    private float faceDirection = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null) Debug.LogError("NewProjectile requires Rigidbody2D!");
        if (spriteRenderer == null) Debug.LogError("NewProjectile requires SpriteRenderer!");
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        SetProjectileSprite();
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

    private void SetProjectileSprite()
    {
        if (spriteRenderer != null && projectileSprites != null && projectileSprites.Length > 0)
        {
            nextSpriteIndex = nextSpriteIndex % projectileSprites.Length;

            spriteRenderer.sprite = projectileSprites[nextSpriteIndex];
            Debug.Log($"Assigned sprite: {spriteRenderer.sprite.name} at index {nextSpriteIndex}", gameObject);

            // Increment for next shot
            nextSpriteIndex = (nextSpriteIndex + 1) % projectileSprites.Length;
        }
        else if (spriteRenderer != null)
        { 
            Debug.LogWarning("No projectile sprites assigned or array is empty!", gameObject);
        }
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
