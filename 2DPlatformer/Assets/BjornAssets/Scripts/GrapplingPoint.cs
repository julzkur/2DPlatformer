using UnityEngine;

public class GrapplingPoint : MonoBehaviour
{
    // Visual indicator when player is in range
    public GameObject inRangeIndicator;
    
    void Start()
    {
        // Ensure this object has the correct tag
        gameObject.tag = "GrapplePoint";
        
        // Create a visual indicator if one doesn't exist
        if (inRangeIndicator == null)
        {
            inRangeIndicator = new GameObject("RangeIndicator");
            inRangeIndicator.transform.parent = transform;
            inRangeIndicator.transform.localPosition = Vector3.zero;
            
            // Add a sprite renderer to the indicator
            SpriteRenderer indicatorRenderer = inRangeIndicator.AddComponent<SpriteRenderer>();
            indicatorRenderer.sprite = Resources.Load<Sprite>("Circle"); // Default sprite
            indicatorRenderer.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
            indicatorRenderer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        
        // Hide indicator by default
        inRangeIndicator.SetActive(false);

        // Ensure there's a collider
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }
    }
    
    // Player detection using triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRangeIndicator.SetActive(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRangeIndicator.SetActive(false);
        }
    }
} 