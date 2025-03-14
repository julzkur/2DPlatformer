using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;

    [Header("Movement")]
    public float MoveSpeed = 5f;
    private float lastMoveDirection = 1f; 



    [Header("Jump")]
    public float JumpForce = 5f;
    public float DblJumpMultiplier = 0.5f;
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;
    public float GravityScale = 2.5f;

    public bool isGrounded;
    public bool canDblJump;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;
    }

    void Update()
    {

        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer);


        if (isGrounded) 
        {
            canDblJump = true;
        }

        // Jump

        if (Input.GetKeyDown(KeyCode.Space)) {
            
            if (isGrounded) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            } else if (canDblJump) 
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
        } // add or remove slide

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

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
        
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        ToolProjectile projScript = projectile.GetComponent<ToolProjectile>();

        if (projScript != null)
        {
            projScript.SetDirection(lastMoveDirection);
        }
    }
}
