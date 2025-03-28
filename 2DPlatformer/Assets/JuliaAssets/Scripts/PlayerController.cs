
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    GameController gameController;

    [Header("Movement")]
    public float MoveSpeed = 5f;
    private float lastMoveDirection = 1f; 
    private int health = 5;

    [Header("Jump")]
    public float JumpForce = 8f;
    public float DblJumpMultiplier = 0.5f;
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;
    public LayerMask OneWayLayer;
    public float GravityScale = 2.5f;
    public bool isGrounded;
    private bool canDblJump;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float throwForce = 10f;
    public float maxThrowDistance = 20f;
    private float holdTime;
    private LineRenderer trajectoryLine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;

        trajectoryLine = GetComponent<LineRenderer>();
        trajectoryLine.positionCount = 0;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.green;
    }

    void Update()
    {

        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer + OneWayLayer);


        if (isGrounded) 
        {
            canDblJump = true;
        }

        // Jump

        if (Input.GetKeyDown(KeyCode.Space)) {
            
            if (isGrounded) 
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            } 
            else if (canDblJump) 
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce * DblJumpMultiplier);
                canDblJump = false;
            }
        }

        // Affect how gravity affects player when moving either up or down along y axis

        if (rb.linearVelocity.y < 0) 
        {
            rb.gravityScale = GravityScale * 2f;  
        } 
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space)) 
        {
            rb.gravityScale = GravityScale * 1.5f;  
        } 
        else 
        {
            rb.gravityScale = GravityScale;
        }

        // Movement Left or Right
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * MoveSpeed, rb.linearVelocity.y);

        if (moveInput != 0)
        {
            lastMoveDirection = Mathf.Sign(moveInput); // 1 if moving right, -1 if moving left
        } 


        // Flips sprite when moving left or right
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // right
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // left
        }


        // Shooting

        if (Input.GetMouseButtonDown(0))
        {
            holdTime = 0f;
            trajectoryLine.positionCount = 20;
        }
        if (Input.GetMouseButton(0))
        {
            holdTime += Time.deltaTime;
            UpdateTrajectoryLine();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
            trajectoryLine.positionCount = 0;
        }
    }

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Ladder"))
    //     {
    //         Climb();
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyProjectile"))
        {
            TakeDamage();
        }

    }

    void Climb()
    {
        float moveInput = Input.GetAxis("Vertical");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveInput * MoveSpeed);
        rb.gravityScale = 0;
    }

    void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) return;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        ToolProjectile projScript = projectile.GetComponent<ToolProjectile>();

        if (projScript != null)
        {
            Debug.Log("hold time:" + holdTime);
            projScript.SetDirectionandForce(holdTime, throwForce, maxThrowDistance, lastMoveDirection);
        }
    }

    void UpdateTrajectoryLine()
    {
        Vector2 throwDirection = transform.localScale;

        // Predict trajectory points
        for (int i = 0; i < trajectoryLine.positionCount; i++)
        {
            float time = i * 0.5f;
            Vector2 predictedPosition = (Vector2)firePoint.position + throwDirection * Mathf.Min(
                holdTime * throwForce, maxThrowDistance) * time + 0.5f * Physics2D.gravity * time * time;
            trajectoryLine.SetPosition(i, predictedPosition);
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        gameObject.SetActive(false);

        gameController.Respawn(1f);

        gameObject.SetActive(true);
    }
}
