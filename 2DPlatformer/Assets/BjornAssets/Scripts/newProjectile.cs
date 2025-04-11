using UnityEngine;

public class NewProjectile : MonoBehaviour
{
    // Basic Properties (Match ToolProjectile)
    public float lifetime = 3f;
    public float rotationSpeed = 360f;
    public float gravityScale = 1f; 

    // Sprite Cycling
    [Header("Visuals")]
    public Sprite[] projectileSprites = new Sprite[3]; // Assign 3 sprites in Inspector
    private SpriteRenderer spriteRenderer;
    private static int nextSpriteIndex = 0; 

    // Internal Physics
    private Rigidbody2D rb;
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
        if (rb != null) {
            rb.gravityScale = gravityScale;
        }

        SetProjectileSprite();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Rotation logic (Match ToolProjectile)
        if (rb != null) { 
             transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * -faceDirection);
 
        }
    }

    // Launch Method (Match Trajectory Prediction)
    public void SetDirectionAndForce(float holdTime, float throwForce, float maxThrowDistance, float dir) 
    {
        if (rb == null) {
            Debug.LogError("Rigidbody2D is missing on projectile!");
            return;
        }

        faceDirection = dir; 

        // Flip visual scale based on direction
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * dir, currentScale.y, currentScale.z);

        Vector2 launchDirection = new Vector2(dir, 1).normalized; 
        float forceMagnitude = Mathf.Min(holdTime * throwForce, maxThrowDistance);
        Vector2 launchVelocity = launchDirection * forceMagnitude;
  
        rb.linearVelocity = launchVelocity;

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
        // Match ToolProjectile: Ignore Player, destroy on anything else
        if (!collision.CompareTag("Player")) {


             Destroy(gameObject);
        }
    }
} 