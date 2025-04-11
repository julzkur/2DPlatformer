
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    AudioManager audioManager;
    GameController gameController;

    [Header("Movement")]
    public float MoveSpeed = 5f;
    private bool isFacingRight = true;
    private float horizontal;

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
    public float swingForce = 1.5f;
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
    }
    void Start()
    {
        gameController = GameController.Instance;
        rb.gravityScale = GravityScale;
        rb.freezeRotation = true;

        ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
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
        trajectoryLine.enabled = true;
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

        if (Input.GetKeyDown(KeyCode.Space)) 
        {    
            Jump();
        }

        CheckForGrapplePoints();

        if (Input.GetKeyDown(grappleKey))
        {
            Grapple();
            
        }
        
        // Update rope visual if grappling
        if (isGrappling)
        {
            UpdateRopeVisual();
            HandleSwinging();
            GrappleLaunch();
        }

        FallEffect();

        horizontal = Input.GetAxis("Horizontal");
        Flip();
        
        // Shooting
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

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * MoveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        
        if (isGrounded) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            audioManager.PlaySFX(audioManager.jump);
        } 
        else if (canDblJump) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce * DblJumpMultiplier);
            // make jump audio higher pitched
            audioManager.PlaySFX(audioManager.jump);
            canDblJump = false;
        }
    }

    void Grapple()
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

    void GrappleLaunch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 ropeDir = (ropeJoint.connectedAnchor - (Vector2)transform.position).normalized;
            Vector2 perpendicular = new Vector2(ropeDir.y, -ropeDir.x);
            Vector2 launch = rb.linearVelocity + perpendicular * 4f; // small launch push
            StopGrapple();
            rb.linearVelocity = launch;
            return;
        }
    }

    void FallEffect()
    {
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
    }

    void Flip()
    {
        if (isFacingRight && horizontal < 0 || !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    void StartChargingShot()
    {
        holdTime = 0f;
        isChargingShot = true;
        trajectoryLine.positionCount = 20;
    }

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
        audioManager.PlaySFX(audioManager.playerShoot);
        ToolProjectile projScript = projectile.GetComponent<ToolProjectile>();

        if (projScript != null)
        {
            projScript.SetDirectionandForce(holdTime, throwForce, maxThrowDistance, shootDirection);
        }
    }

    void UpdateTrajectoryLine()
    {
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

    public void TakeDamage()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(1);
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

            if (ropeJoint == null)
            {
                ropeJoint = gameObject.AddComponent<DistanceJoint2D>();
            }

            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.connectedAnchor = grapplePoint;
            ropeJoint.enableCollision = true;
            ropeJoint.autoConfigureDistance = false;
            ropeJoint.distance = Vector2.Distance(transform.position, grapplePoint);
            ropeJoint.enabled = true;

            // Activate rope visual
            ropeRenderer.enabled = true;
            ropeRenderer.positionCount = 2;
        }
    }
    
    void StopGrapple()
    {
        isGrappling = false;
        ropeJoint.enabled = false;
        ropeRenderer.enabled = false;
        ropeRenderer.positionCount = 0;
    }
    
    void UpdateRopeVisual()
    {
        if (ropeRenderer.positionCount < 2)
            ropeRenderer.positionCount = 2;

        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, ropeJoint.connectedAnchor);
    }

    
    void HandleSwinging()
    {
        if (!isGrappling) return;

        Vector2 toGrapplePoint = ropeJoint.connectedAnchor - (Vector2)transform.position;
        Vector2 ropeDir = toGrapplePoint.normalized;

        Vector2 swingDir = new Vector2(ropeDir.y, -ropeDir.x);

        float moveInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            rb.AddForce(swingDir * moveInput * swingForce, ForceMode2D.Force);
        }

        float currentDistance = toGrapplePoint.magnitude;
        float desiredDistance = ropeJoint.distance;

        if (currentDistance > desiredDistance * 0.95f)
        {
            Vector2 tensionForce = ropeDir * (currentDistance - desiredDistance) * 30f;
            rb.AddForce(tensionForce, ForceMode2D.Force);
        }
    }
}
