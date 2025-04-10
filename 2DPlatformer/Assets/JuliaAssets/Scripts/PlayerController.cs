
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    GameController gameController;

    [Header("Movement")]
    public float MoveSpeed = 5f;
    private float lastMoveDirection = 1f; 

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
    private int shootDirection = 1; // 1 = right, -1 = left
    private bool isChargingShot = false;

    public float projectileSpeed = 10f;
    public float throwForce = 10f;
    public float maxThrowDistance = 20f;
    private float holdTime;
    private LineRenderer trajectoryLine;

    [Header("Grappling Hook")]
    public float grappleRange = 5f;
    public float swingForce = 1.5f;
    public KeyCode grappleKey = KeyCode.E;
    public LayerMask grappableLayer;
    public GameObject grapplePointPrefab;
    public LineRenderer ropeRenderer;
    public float momentumRetention = 0.9f;  // How much momentum to keep after detaching (0-1)
    
    // Grappling internals
    private bool isGrappling = false;
    private Vector2 grapplePoint;
    private DistanceJoint2D ropeJoint;
    private GameObject[] grapplePointsInRange = new GameObject[10]; // Pre-allocate array
    private int grapplePointCount = 0;

    void Start()
    {
        gameController = GameController.Instance;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;

        // ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
        // ropeJoint.enabled = false;
        
        // // Setup line renderer if not assigned
        // if (ropeRenderer == null)
        // {
        //     ropeRenderer = gameObject.AddComponent<LineRenderer>();
        //     ropeRenderer.startWidth = 0.05f;
        //     ropeRenderer.endWidth = 0.05f;
        //     ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        //     ropeRenderer.startColor = Color.black;
        //     ropeRenderer.endColor = Color.black;
        //     ropeRenderer.positionCount = 2;
        //     ropeRenderer.enabled = false;
        // }


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
        

        if (moveInput != 0)
        {
            rb.linearVelocity = new Vector2(moveInput * MoveSpeed, rb.linearVelocity.y);
            lastMoveDirection = Mathf.Sign(moveInput); // 1 if moving right, -1 if moving left
        } 
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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

        // Start charging shot
        if (Input.GetKeyDown(KeyCode.J))
        {
            shootDirection = -1;
            holdTime = 0f;
            isChargingShot = true;
            trajectoryLine.positionCount = 20;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            shootDirection = 1;
            holdTime = 0f;
            isChargingShot = true;
            trajectoryLine.positionCount = 20;
        }

        // While holding J or K, keep charging and update line
        if (isChargingShot && (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.K)))
        {
            holdTime += Time.deltaTime;
            UpdateTrajectoryLine();
        }

        // Releasing either key fires
        if (isChargingShot && (Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.K)))
        {
            Shoot();
            trajectoryLine.positionCount = 0;
            isChargingShot = false;
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
            projScript.SetDirectionandForce(holdTime, throwForce, maxThrowDistance, shootDirection);
        }
    }


    void UpdateTrajectoryLine()
    {
        Vector2 throwDirection = new Vector2(shootDirection, 1).normalized;

        for (int i = 0; i < trajectoryLine.positionCount; i++)
        {
            float time = i * 0.1f;
            Vector2 predictedPosition = (Vector2)firePoint.position +
                throwDirection * Mathf.Min(holdTime * throwForce, maxThrowDistance) * time +
                0.5f * Physics2D.gravity * time * time;

            trajectoryLine.SetPosition(i, predictedPosition);
        }
    }

    public void TakeDamage()
    {
        if (PlayerHealthHearts.Instance != null)
        {
            PlayerHealthHearts.Instance.TakeDamage(1);
        }
    }
}
