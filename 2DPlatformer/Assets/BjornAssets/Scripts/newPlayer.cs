using UnityEngine;
using System.Collections;

public class NewPlayer : MonoBehaviour
{
    // Core Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; 

    // Movement
    [Header("Movement")]
    public float MoveSpeed = 5f;
    private float lastMoveDirection = 1f; 

    // Jump
    [Header("Jump")]
    public float JumpForce = 8f;
    public float DblJumpMultiplier = 0.5f;
    public Transform GroundCheck; 
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;
    public LayerMask OneWayLayer; 
    public float GravityScale = 2.5f; 
    private bool isGrounded;
    private bool canDblJump;

    // Shooting (Mouse Input)
    [Header("Shooting")]
    public GameObject newProjectilePrefab; 
    public Transform firePoint;      
    public float throwForce = 10f;
    public float maxThrowDistance = 20f; 
    private float holdTime;
    private bool isChargingShot = false;
    private LineRenderer trajectoryLine;

    // Grappling Hook
    [Header("Grappling Hook")]
    public float grappleRange = 5f;
    public float swingForce = 4f;
    public KeyCode grappleKey = KeyCode.E;
    public LayerMask grappableLayer; 
    private bool isGrappling = false;
    private Vector2 grapplePoint;
    private DistanceJoint2D ropeJoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Transform childSprite = transform.Find("PlayerSprite");
        if (childSprite != null) {
            spriteRenderer = childSprite.GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        trajectoryLine = GetComponent<LineRenderer>(); // Get the LineRenderer for trajectory/rope
        ropeJoint = GetComponent<DistanceJoint2D>();  

        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (animator == null) Debug.LogWarning("Animator not found!"); 
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found on Player or child PlayerSprite!");
        if (trajectoryLine == null) Debug.LogWarning("LineRenderer not found!"); 
        if (GroundCheck == null) Debug.LogError("GroundCheck Transform not assigned!");
        if (firePoint == null) Debug.LogError("FirePoint Transform not assigned!");
    }

    void Start()
    {
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;

        // Configure LineRenderer
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.startWidth = 0.1f;
            trajectoryLine.endWidth = 0.1f;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.enabled = false;
        }

        // Configure Rope Joint (Add if needed)
        if (ropeJoint == null) {
            ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
            ropeJoint.enableCollision = true;
        }
        ropeJoint.enabled = false; 
    }

    void Update()
    {
        // --- Ground Check ---
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer | OneWayLayer);
        if (isGrounded) {
            canDblJump = true;
        }

        // --- State Machine Logic ---
        if (isGrappling) {
            HandleGrapplingState();
        } else {
            HandleDefaultState();
        }

        // --- Animator Updates (Always run) ---
        if (animator != null) {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x)); 
        }
    }

    // Handles movement, jumping, shooting, starting grapple
    void HandleDefaultState()
    {
        // Movement
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * MoveSpeed, rb.linearVelocity.y);
        if (moveInput != 0) {
            lastMoveDirection = Mathf.Sign(moveInput);
        }

        // Sprite Flipping (Using Scale like reference)
        HandleSpriteFlip(moveInput);

        // Jumping
        HandleJumpInput();

        // Shooting
        HandleShootingInput();

        // Gravity Scaling
        ApplyGravityScaling();

        // Check if starting grapple
        if (Input.GetKeyDown(grappleKey) && !isChargingShot) {
            CheckForGrapplePointsAndStart();
        }
    }

    // Handles swinging, letting go
    void HandleGrapplingState()
    {
        if (Input.GetKeyDown(grappleKey)) {
            StopGrapple();
            return;
        }

        // Swinging
        float swingInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(swingInput) > 0.1f) {
            Vector2 ropeDirection = (grapplePoint - (Vector2)transform.position).normalized;
            Vector2 perpendicularDirection = new Vector2(ropeDirection.y, -ropeDirection.x);
            rb.AddForce(perpendicularDirection * swingInput * swingForce, ForceMode2D.Force);
        }

        UpdateRopeVisual();
    }

    // --- Input Helper Methods ---

    void HandleJumpInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (isGrounded) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                if (animator != null) animator.SetTrigger("Jump");
            } else if (canDblJump) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce * DblJumpMultiplier);
                canDblJump = false;
                if (animator != null) animator.SetTrigger("Jump");
            }
        }
    }

    void HandleShootingInput() {
        if (Input.GetMouseButtonDown(0)) {
            holdTime = 0f;
            isChargingShot = true;
            if (trajectoryLine != null) {
                trajectoryLine.positionCount = 20; // Set points for drawing
                trajectoryLine.startColor = Color.green;
                trajectoryLine.endColor = Color.green;
                trajectoryLine.enabled = true;
                UpdateTrajectoryLine(); 
            }
        }

        if (Input.GetMouseButton(0) && isChargingShot) {
            holdTime += Time.deltaTime;
            UpdateTrajectoryLine();
        }

        if (Input.GetMouseButtonUp(0) && isChargingShot) {
            Shoot();
            isChargingShot = false;
            if (trajectoryLine != null) {
                trajectoryLine.positionCount = 0;
                trajectoryLine.enabled = false;
            }
        }
    }

    void HandleSpriteFlip(float moveInput) {
         // Ensure spriteRenderer is not null before flipping
         if (moveInput > 0.1f) {
              transform.localScale = new Vector3(1, 1, 1);
              lastMoveDirection = 1; 
         } else if (moveInput < -0.1f) { 
              transform.localScale = new Vector3(-1, 1, 1);
              lastMoveDirection = -1; 
         }

         // Alternative using SpriteRenderer.flipX (if scale causes issues):
         // if (spriteRenderer != null) {
         //     if (Mathf.Abs(moveInput) > 0.1f) {
         //         spriteRenderer.flipX = (moveInput < 0);
         //     }
         // }
    }

    void ApplyGravityScaling() {
        if (rb.linearVelocity.y < 0) {
            rb.gravityScale = GravityScale * 2f;
        } else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space)) {
            rb.gravityScale = GravityScale * 1.5f;
        } else {
            rb.gravityScale = GravityScale;
        }
    }


    // --- Action Methods ---

    void Shoot() {
        if (firePoint == null || newProjectilePrefab == null) {
            Debug.LogError("FirePoint or Projectile Prefab not set!");
            return;
        }
        GameObject projectile = Instantiate(newProjectilePrefab, firePoint.position, Quaternion.identity);
        NewProjectile projScript = projectile.GetComponent<NewProjectile>();
        if (projScript != null) {
            projScript.SetDirectionAndForce(holdTime, throwForce, maxThrowDistance, lastMoveDirection);
        } else {
            Debug.LogError("Instantiated projectile missing NewProjectile script!");
        }
    }

    //Trajectory Calculation 
    void UpdateTrajectoryLine() {
        if (trajectoryLine == null || !isChargingShot) {
            if (trajectoryLine != null) { 
                 trajectoryLine.positionCount = 0;
                 trajectoryLine.enabled = false;
            }
            return; 
        }

        // Ensure line has points and is enabled
        if (trajectoryLine.positionCount != 20) trajectoryLine.positionCount = 20;
        if (!trajectoryLine.enabled) trajectoryLine.enabled = true;

        Vector2 launchDirection = new Vector2(lastMoveDirection, 1).normalized; 
        float forceMagnitude = Mathf.Min(holdTime * throwForce, maxThrowDistance);
        Vector2 initialVelocity = launchDirection * forceMagnitude;

        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.green;

        float projectileGravityScale = 1.0f; 

        for (int i = 0; i < trajectoryLine.positionCount; i++) {
            float time = i * 0.1f; 
            Vector2 displacement = initialVelocity * time + 0.5f * Physics2D.gravity * projectileGravityScale * time * time;
            Vector2 predictedPosition = (Vector2)firePoint.position + displacement;
            trajectoryLine.SetPosition(i, predictedPosition);
        }
    }


    // --- Grappling Methods ---

    void CheckForGrapplePointsAndStart() {
         Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grappleRange, grappableLayer);
         GameObject closestPoint = null;
         float minDist = float.MaxValue;

         foreach (Collider2D hit in hits) {
             if (hit.CompareTag("GrapplePoint")) {
                 float dist = Vector2.Distance(transform.position, hit.transform.position);
                 if (dist < minDist) {
                     minDist = dist;
                     closestPoint = hit.gameObject;
                 }
             }
         }

         if (closestPoint != null) {
             StartGrapple(closestPoint.transform.position);
         }
    }

    void StartGrapple(Vector2 targetPoint) {
        if (isGrappling) return; // Already grappling

        isGrappling = true;
        grapplePoint = targetPoint;
        if (animator != null) animator.SetBool("IsGrappling", true);

        // Configure and enable joint
        ropeJoint.connectedAnchor = grapplePoint;
        ropeJoint.distance = Vector2.Distance(transform.position, grapplePoint) * 0.9f; 
        ropeJoint.enabled = true;

        // Show rope
        if (trajectoryLine != null) { 
            trajectoryLine.positionCount = 2;
            trajectoryLine.startColor = Color.yellow; // Change color for rope
            trajectoryLine.endColor = Color.yellow;
            trajectoryLine.SetPosition(0, transform.position);
            trajectoryLine.SetPosition(1, grapplePoint);
            trajectoryLine.enabled = true;
        }

        rb.gravityScale = GravityScale * 0.5f;
    }

    void StopGrapple() {
        isGrappling = false;
        if (animator != null) animator.SetBool("IsGrappling", false);
        ropeJoint.enabled = false;

        // Hide rope/trajectory line
        if (trajectoryLine != null) {
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }
        // Restore gravity
        rb.gravityScale = GravityScale;
    }

    void UpdateRopeVisual() {
        if (trajectoryLine != null && trajectoryLine.enabled && trajectoryLine.positionCount == 2 && isGrappling) {
            trajectoryLine.SetPosition(0, transform.position);
            trajectoryLine.SetPosition(1, grapplePoint); 
        }
    }

    // --- Health & Collision --- 
    public void TakeDamage() {
        if (PlayerHealth.Instance != null) {
            PlayerHealth.Instance.TakeDamage(1);
        } else {
            Debug.LogWarning("PlayerHealthHearts instance not found for TakeDamage call.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("EnemyProjectile")) {
            TakeDamage();
        }
    }

    // --- Utility & Debug ---
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        if (GroundCheck != null) Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }

    public bool IsGrappling() { return isGrappling; }
    public void OnAnimationEvent() { /* Intentionally empty */ }
}
