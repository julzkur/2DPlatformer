
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator animator;
    AudioManager audioManager;
    GameController gameController;
    private SpriteRenderer spriteRenderer; 

    [Header("Movement")]
    public float MoveSpeed = 5f;
    private bool isFacingRight = true;
    private float horizontal;
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
    [SerializeField] LineRenderer trajectoryLine;

    [Header("Grappling Hook")]
    public float grappleRange = 5f;
    public float swingForce = 4f;
    public KeyCode grappleKey = KeyCode.E;
    public LayerMask grappableLayer;
    public GameObject grapplePointPrefab;
    [SerializeField] LineRenderer ropeRenderer;
    public float momentumRetention = 0.9f;  // How much momentum to keep after detaching (0-1)
    
    // Grappling internals
    private bool isGrappling = false;
    private Vector2 grapplePoint;
    private DistanceJoint2D ropeJoint;
    private GameObject[] grapplePointsInRange = new GameObject[10]; // Pre-allocate array
    private int grapplePointCount = 0;

    void Awake()
    {
        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
        }
        rb = GetComponent<Rigidbody2D>();

        animator = transform.Find("PlayerSprite").GetComponent<Animator>();
        Transform childSprite = transform.Find("PlayerSprite");
        // if (childSprite != null) {
        //     spriteRenderer = childSprite.GetComponent<SpriteRenderer>();
        // }
        // if (spriteRenderer == null) {
        //     spriteRenderer = GetComponent<SpriteRenderer>();
        // }

        // trajectoryLine = GetComponent<LineRenderer>(); // Get the LineRenderer for trajectory/rope
        ropeJoint = GetComponent<DistanceJoint2D>();  

        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (animator == null) Debug.LogWarning("Animator not found!"); 
        // if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found on Player or child PlayerSprite!");
        if (trajectoryLine == null) Debug.LogWarning("LineRenderer not found!"); 
        if (GroundCheck == null) Debug.LogError("GroundCheck Transform not assigned!");
        if (firePoint == null) Debug.LogError("FirePoint Transform not assigned!");
    }
    void Start()
    {
        gameController = GameController.Instance;
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;

        ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
        ropeJoint.enableCollision = true;
        ropeJoint.enabled = false;
        
        // Setup line renderer if not assigned
        if (ropeRenderer == null)
        {
            Debug.LogError("Rope Renderer not assigned in the inspector.");
            return;
        }

        ropeRenderer.startWidth = 0.05f;
        ropeRenderer.endWidth = 0.05f;
        ropeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        ropeRenderer.startColor = Color.yellow;
        ropeRenderer.endColor = Color.yellow;
        ropeRenderer.positionCount = 2;
        ropeRenderer.enabled = false;

        if (trajectoryLine == null)
        {
            Debug.LogError("Trajectory Line Renderer not assigned in the inspector.");
            return;
        }
        trajectoryLine.enabled = false;
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
        Flip();

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

    void HandleGrapplingState()
    {
        // Cancel grapple with key press
        if (Input.GetKeyDown(grappleKey) || Input.GetKeyDown(KeyCode.Space))
        {
            StopGrapple();
            return;
        }

        Vector2 toGrapple = grapplePoint - (Vector2)transform.position;
        float currentLength = toGrapple.magnitude;
        float desiredLength = ropeJoint.distance;

        // Apply swing input (perpendicular to rope)
        float swingInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(swingInput) > 0.1f)
        {
            Vector2 swingDir = new Vector2(toGrapple.y, -toGrapple.x).normalized;
            rb.AddForce(swingDir * swingInput * swingForce, ForceMode2D.Force);
        }

        // Apply simulated rope tension if stretched
        if (currentLength > desiredLength * 0.95f)
        {
            float stretchFactor = currentLength - desiredLength;
            Vector2 tensionForce = toGrapple.normalized * stretchFactor * 50f;
            rb.AddForce(tensionForce);
        }

        UpdateRopeVisual();
    }


    // --- Input Helper Methods ---

    void HandleJumpInput() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (isGrounded) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                audioManager.PlaySFX(audioManager.jump);
                if (animator != null) animator.SetTrigger("Jump");
            } else if (canDblJump) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce * DblJumpMultiplier);
                canDblJump = false;
                // make jump audio higher pitched
                audioManager.PlaySFX(audioManager.jump);
                if (animator != null) animator.SetTrigger("Jump");
            }
        }
    }

    void HandleShootingInput() {
        if (!isChargingShot)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                shootDirection = -1;
                StartChargingShot();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                shootDirection = 1;
                StartChargingShot();
            }
        }

        if (isChargingShot && (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.K)))
        {
            holdTime += Time.deltaTime;
            UpdateTrajectoryLine();
        }

        if (isChargingShot && (Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.K)))
        {
            Shoot();
            trajectoryLine.positionCount = 0;
            isChargingShot = false;
        }
    }
    
    void Flip()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0 && !isFacingRight)
        {
            FlipSprite();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
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

    void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) {
            Debug.LogError("FirePoint or Projectile Prefab not set!");
            return;
        }
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        audioManager.PlaySFX(audioManager.playerShoot);
        ToolProjectile projScript = projectile.GetComponent<ToolProjectile>();

        if (projScript != null)
        {
            projScript.SetDirectionandForce(holdTime, throwForce, maxThrowDistance, shootDirection);
        } else {
            Debug.LogError("Instantiated projectile missing NewProjectile script!");
        }
    }

    void UpdateTrajectoryLine()
    {
        if (trajectoryLine == null || !isChargingShot) {
            if (trajectoryLine != null) { 
                 trajectoryLine.positionCount = 0;
                 trajectoryLine.enabled = false;
            }
            return; 
        }

        Vector2 throwDirection = new Vector2(shootDirection, 1).normalized;
        float force = Mathf.Min(holdTime * throwForce, maxThrowDistance);
        Vector2 initialVelocity = throwDirection * force;

        for (int i = 0; i < trajectoryLine.positionCount; i++)
        {
            float time = i * 0.1f;
            Vector2 predictedPosition = (Vector2)firePoint.position +
                initialVelocity * time +
                0.5f * Physics2D.gravity * time * time;

            trajectoryLine.SetPosition(i, predictedPosition);
        }
    }

    // --- Grappling Methods ---

    void CheckForGrapplePointsAndStart()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grappleRange, grappableLayer);
        GameObject closestPoint = null;
        float minDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("GrapplePoint")) continue;

            Vector2 direction = hit.transform.position - transform.position;
            RaycastHit2D block = Physics2D.Linecast(transform.position, hit.transform.position, GroundLayer | OneWayLayer);

            if (block.collider != null)
            {
                Debug.DrawLine(transform.position, hit.transform.position, Color.red, 0.5f);
                continue; // Line blocked by something in Ground/OneWay layers
            }

            float dist = direction.sqrMagnitude; // use sqr for performance
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = hit.gameObject;
            }
        }

        if (closestPoint != null)
        {
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
        if (ropeRenderer != null) { 
            ropeRenderer.positionCount = 2;
            ropeRenderer.startColor = Color.yellow; // Change color for rope
            ropeRenderer.endColor = Color.yellow;
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grapplePoint);
            ropeRenderer.enabled = true;
        }

        rb.gravityScale = GravityScale * 0.5f;
    }

    

    void StopGrapple() {
        isGrappling = false;
        if (animator != null) animator.SetBool("IsGrappling", false);
        ropeJoint.enabled = false;

        // Hide rope/trajectory line
        if (ropeRenderer != null) {
            ropeRenderer.positionCount = 0;
            ropeRenderer.enabled = false;
        }
        // Restore gravity
        rb.gravityScale = GravityScale;
        canDblJump = true;
    }


    void UpdateRopeVisual() {
        if (ropeRenderer != null && ropeRenderer.enabled && ropeRenderer.positionCount == 2 && isGrappling) {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grapplePoint); 
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

    void StartChargingShot()
    {
        holdTime = 0f;
        isChargingShot = true;
        trajectoryLine.positionCount = 20;
        trajectoryLine.enabled = true;
    }

    public bool IsGrappling() { return isGrappling; }
    public void OnAnimationEvent() { /* Intentionally empty */ }


///////////////////////////////////////////// IGNOE ///////////////////////////////////
    // --- Input Helper Methods ---

    // void Update2() {
        // if (Input.GetKeyDown(KeyCode.Space)) 
        // {    
        //     Jump();
        // }

        // // CheckForGrapplePoints();

        // if (Input.GetKeyDown(grappleKey))
        // {
        //     // Grapple();
            
        // }
        
        // // Update rope visual if grappling
        // if (isGrappling)
        // {
        //     UpdateRopeVisual();
        //     // HandleSwinging();
        //     GrappleLaunch();
        // }

        // horizontal = Input.GetAxis("Horizontal");
        // Flip();
    // }

    // void FixedUpdate()
    // {
    //     rb.linearVelocity = new Vector2(horizontal * MoveSpeed, rb.linearVelocity.y);
    // }

    // void Jump()
    // {
        
    //     if (isGrounded) 
    //     {
    //         rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
    //         audioManager.PlaySFX(audioManager.jump);
    //     } 
    //     else if (canDblJump) 
    //     {
    //         rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce * DblJumpMultiplier);
    //         // make jump audio higher pitched
    //         audioManager.PlaySFX(audioManager.jump);
    //         canDblJump = false;
    //     }
    // }

    // void Grapple()
    // {
    //     if (!isGrappling && grapplePointCount > 0)
    //     {
    //         StartGrapple();
    //     }
    //     else if (isGrappling)
    //     {
    //         Vector2 velocityBeforeDetach = rb.linearVelocity;
    //         StopGrapple();
            
    //         // Preserve momentum when detaching
    //         rb.linearVelocity = velocityBeforeDetach * momentumRetention;
    //     }
    // }

    // void GrappleLaunch()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         Vector2 ropeDir = (ropeJoint.connectedAnchor - (Vector2)transform.position).normalized;
    //         Vector2 perpendicular = new Vector2(ropeDir.y, -ropeDir.x);
    //         Vector2 launch = rb.linearVelocity + perpendicular * 4f; // small launch push
    //         StopGrapple();
    //         rb.linearVelocity = launch;
    //         return;
    //     }
    // }

    // void Climb()
    // {
    //     float moveInput = Input.GetAxis("Vertical");
    //     rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveInput * MoveSpeed);
    //     rb.gravityScale = 0;
    // }

    // void StartGrapple()
    // {
    //     // Find the closest grapple point
    //     GameObject closestPoint = null;
    //     float closestDistance = float.MaxValue;
        
    //     for (int i = 0; i < grapplePointCount; i++)
    //     {
    //         GameObject point = grapplePointsInRange[i];
    //         float distance = Vector2.Distance(transform.position, point.transform.position);
    //         if (distance < closestDistance)
    //         {
    //             closestDistance = distance;
    //             closestPoint = point;
    //         }
    //     }

    //     if (closestPoint != null)
    //     {
    //         isGrappling = true;
    //         grapplePoint = closestPoint.transform.position;

    //         if (ropeJoint == null)
    //         {
    //             ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
    //         }

    //         ropeJoint.autoConfigureConnectedAnchor = false;
    //         ropeJoint.connectedAnchor = grapplePoint;
    //         ropeJoint.enableCollision = true;
    //         ropeJoint.autoConfigureDistance = false;
    //         ropeJoint.distance = Vector2.Distance(transform.position, grapplePoint);
    //         ropeJoint.enabled = true;

    //         // Activate rope visual
    //         ropeRenderer.enabled = true;
    //         ropeRenderer.positionCount = 2;
    //     }
    // }

    // void CheckForGrapplePoints()
    // {
    //     // Reset count
    //     grapplePointCount = 0;
        
    //     // Find all grapple points in range
    //     Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, grappleRange, grappableLayer);
    //     foreach (Collider2D collider in hitColliders)
    //     {
    //         if (grapplePointCount < grapplePointsInRange.Length)
    //         {
    //             grapplePointsInRange[grapplePointCount] = collider.gameObject;
    //             grapplePointCount++;
    //         }
    //     }
    // }

    
    

    
    // void HandleSwinging()
    // {
    //     if (!isGrappling) return;

    //     Vector2 toGrapplePoint = ropeJoint.connectedAnchor - (Vector2)transform.position;
    //     Vector2 ropeDir = toGrapplePoint.normalized;

    //     Vector2 swingDir = new Vector2(ropeDir.y, -ropeDir.x);

    //     float moveInput = Input.GetAxis("Horizontal");
    //     if (Mathf.Abs(moveInput) > 0.1f)
    //     {
    //         rb.AddForce(swingDir * moveInput * swingForce, ForceMode2D.Force);
    //     }

    //     float currentDistance = toGrapplePoint.magnitude;
    //     float desiredDistance = ropeJoint.distance;

    //     if (currentDistance > desiredDistance * 0.95f)
    //     {
    //         Vector2 tensionForce = ropeDir * (currentDistance - desiredDistance) * 30f;
    //         rb.AddForce(tensionForce, ForceMode2D.Force);
    //     }
    // }
}