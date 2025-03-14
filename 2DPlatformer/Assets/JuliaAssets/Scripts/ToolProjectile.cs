using UnityEngine;

public class ToolProjectile : MonoBehaviour
{
    public float Speed = 10f;
    public float Lifetime = 3f;
    private float direction = 1f;

    private void Start()
    {
        Destroy(gameObject, Lifetime); 
    }

    void Update()
    {
        transform.position += Vector3.right * direction * Speed * Time.deltaTime;
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
            return;
        }
        Destroy(gameObject);
    }
}
