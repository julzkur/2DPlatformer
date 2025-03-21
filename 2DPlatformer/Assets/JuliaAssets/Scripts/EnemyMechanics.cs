using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMechanics : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isChasing = false;
    private float shootTimer = 0f;
    private int health = 5;
    private int patrolIndex = 0;


    [Header("Movement")]
    public float MoveSpeed = 2.5f;
    private float lastMoveDirection = 1f;
    public float hoverDistance = 1f;
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;


    [Header("Patrolling")]
    public Transform[] patrolPoints;
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
        else 
        {
            Patrol();
        }

    }

    void Hover()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, hoverDistance, GroundLayer);

        if (hit.collider != null)
        {
            float height = hit.point.y + hoverDistance;
            transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, height, Time.deltaTime * 5f));
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length < 2) return;

        if (isChasing) return;

        Transform target = patrolPoints[patrolIndex];

        transform.position = Vector2.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);

        //RotateTowardsTarget(target.position);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Attack()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }

        shootTimer -= Time.deltaTime;

    }

    void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) return;


        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        
        if (projRb != null)
        {
            projRb.linearVelocity = firePoint.right * projectileSpeed;
        }

        Destroy(projectile, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile"))
        {
            health--;
            if (health <= 0)
            {
                Destroy(collision.gameObject);
            }
        }

    }

    void RotateTowardsTarget(Vector2 target)
    {
        if (target != null)
        {
            Vector2 direction = target - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), 200f * Time.deltaTime);
            
        }

    }

}
