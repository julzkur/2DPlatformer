using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    private float direction = 1f;


    void Start()
    {
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    public void SetDirection(float dir)
    {
        direction = dir;
        if (dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage();
        }
        Destroy(gameObject);
    }
}
