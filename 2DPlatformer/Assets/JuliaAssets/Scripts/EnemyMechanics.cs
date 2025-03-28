using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMechanics : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isChasing = false;
    private float shootTimer = 0f;
    private int health = 3;


    [Header("Movement")]
    public float MoveSpeed = 2.5f;
    private float lastMoveDirection = 1f;
    public float hoverDistance = 1f;
    public float hoverForce = 50f;
    public float hoverDamping = 5f;
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;


    [Header("Patrolling")]
    public Transform point1;
    public Transform point2;
    private Transform currentPoint;
    public float patrolSpeed = 1.5f;
    public float detectDistance = 5f;


    [Header("Shooting")]
    public GameObject player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float shootCooldown = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        currentPoint = point2.transform;
        lastMoveDirection = (transform.localScale.x >= 0) ? 1f : -1f;
    }

    void Update()
    {
        Hover();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null && Vector2.Distance(transform.position, player.transform.position) < detectDistance)
        {
            isChasing = true;
        } 
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            Attack();
        }


        Patrol();

    }

    void Hover()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, hoverDistance * 2, GroundLayer);

        if (hit.collider != null)
        {
            float distance = hit.distance;
            float forceAmount = (hoverDistance - distance) * hoverForce - rb.linearVelocity.y * hoverDamping;
            rb.AddForce(Vector2.up * forceAmount, ForceMode2D.Force);
        }
    }

    void Patrol()
    {
        Vector2 point = currentPoint.position - transform.position;

        if (currentPoint == point2.transform)
        {
            rb.linearVelocity = new Vector2(patrolSpeed, 0);
        } else {
            rb.linearVelocity = new Vector2(-patrolSpeed, 0);
        }
        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == point2.transform)
        {
            Flip();
            currentPoint = point1.transform;
        } else if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == point1.transform)
        {
            Flip();
            currentPoint = point2.transform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(point1.transform.position, 0.5f);
        Gizmos.DrawWireSphere(point2.transform.position, 0.5f);
        Gizmos.DrawLine(point1.transform.position, point2.transform.position);
    }

    void Attack()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);
        
        Vector2 direction = player.transform.position - transform.position;

        if ((direction.x > 0 && lastMoveDirection < 0) || (direction.x < 0 && lastMoveDirection > 0))
        {
            Flip();
        }

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }

        shootTimer -= Time.deltaTime;
    }

    void Flip()
    {
        lastMoveDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projRb = projectile.GetComponent<EnemyProjectile>();
        
        if (projRb != null)
        {
            projRb.SetDirection(lastMoveDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile"))
        {
            health--;
            Debug.Log("Enemy health: " + health);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }

    }

}
