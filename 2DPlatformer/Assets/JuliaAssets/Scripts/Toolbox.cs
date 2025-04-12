using UnityEngine;

public class Toolbox : MonoBehaviour
{
    public int healthReplenishAmount = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth.Instance.Heal(healthReplenishAmount);

            Destroy(gameObject);
        }
    }
}
