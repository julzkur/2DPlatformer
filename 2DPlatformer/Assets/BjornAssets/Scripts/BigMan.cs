using UnityEngine;

public class BigMan : MonoBehaviour
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
    public float GravityScale = 2.5f;
    public bool isGrounded;
    public bool canDblJump;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

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
    private bool grappleKeyWasPressed = false;  // Track if the key was already pressed
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;
        
        // Setup rope joint
        ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
        ropeJoint.enabled = false;
        
        // Setup line renderer if not assigned
        if (ropeRenderer == null)
        {
            ropeRenderer = gameObject.AddComponent<LineRenderer>();
            ropeRenderer.startWidth = 0.05f;
            ropeRenderer.endWidth = 0.05f;
            ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            ropeRenderer.startColor = Color.black;
            ropeRenderer.endColor = Color.black;
            ropeRenderer.positionCount = 2;
            ropeRenderer.enabled = false;
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer);

        if (isGrounded) 
        {
            canDblJump = true;
        }

        // Check for grapple points
        CheckForGrapplePoints();

        // Handle Toggle-Style Grappling with E key
        if (Input.GetKeyDown(grappleKey))
        {
            if (!isGrappling && grapplePointCount > 0)
            {
                StartGrapple();
            }
            else if (isGrappling)
            {
                Vector2 velocityBeforeDetach = rb.linearVelocity;
                StopGrapple();
                
                // Preserve momentum when detaching
                rb.linearVelocity = velocityBeforeDetach * momentumRetention;
            }
            
        }
        
        // Update rope visual if grappling
        if (isGrappling)
        {
            UpdateRopeVisual();
            HandleSwinging();
        }

        else
        {
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
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.W)) 
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
                lastMoveDirection = Mathf.Sign(moveInput);
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
        }

        // Shooting
        if (Input.GetMouseButtonDown(0) && !isGrappling)
        {
            Shoot();
        }
    }

    void CheckForGrapplePoints()
    {
        // Reset count
        grapplePointCount = 0;
        
        // Find all grapple points in range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, grappleRange, grappableLayer);
        foreach (Collider2D collider in hitColliders)
        {
            if (grapplePointCount < grapplePointsInRange.Length)
            {
                grapplePointsInRange[grapplePointCount] = collider.gameObject;
                grapplePointCount++;
            }
        }
    }

    void StartGrapple()
    {
        // Find the closest grapple point
        GameObject closestPoint = null;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < grapplePointCount; i++)
        {
            GameObject point = grapplePointsInRange[i];
            float distance = Vector2.Distance(transform.position, point.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }
        
        if (closestPoint != null)
        {
            isGrappling = true;
            grapplePoint = closestPoint.transform.position;
            
            // Set up the rope joint
            ropeJoint.enabled = true;
            ropeJoint.connectedAnchor = grapplePoint;
            ropeJoint.distance = Vector2.Distance(transform.position, grapplePoint) * 0.8f; // Shorter than actual distance
            
            // Activate rope visual
            ropeRenderer.enabled = true;
        }
    }
    
    void StopGrapple()
    {
        isGrappling = false;
        ropeJoint.enabled = false;
        ropeRenderer.enabled = false;
    }
    
    void UpdateRopeVisual()
    {
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, grapplePoint);
    }
    
    void HandleSwinging()
    {
        // Calculate swing direction (perpendicular to rope)
        Vector2 ropeDirection = (grapplePoint - (Vector2)transform.position).normalized;
        Vector2 swingDirection = new Vector2(ropeDirection.y, -ropeDirection.x);
        
        // Apply force in the direction of player input
        float moveInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Vector2 forceToApply = swingDirection * moveInput * swingForce;
            rb.AddForce(forceToApply, ForceMode2D.Force);
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
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        ToolProjectile projScript = projectile.GetComponent<ToolProjectile>();

        // if (projScript != null)
        // {
        //     projScript.SetDirectionandForce(lastMoveDirection);
        // }
    }
    
    // Draw grapple range in the editor for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }
}