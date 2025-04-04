using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject[] platforms; // Assign the platforms affected by the lever
    public Color activatedColor = new Color(0.2f, 0.6f, 0.2f); // Dark green
    public Color deactivatedColor = new Color(0.6f, 0.2f, 0.2f); // Dark red
    public GameObject leverOn;
    public GameObject leverOff;

    public bool isActivated = false;

    void Start()
    {
        UpdateLeverState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && PlayerIsNearby())
        {
            isActivated = !isActivated;
            TogglePlatforms(isActivated);
            UpdateLeverState();
        }
    }

    void TogglePlatforms(bool state)
    {
        foreach (GameObject platform in platforms)
        {
            if (platform.TryGetComponent(out PlatformEffector2D effector))
            {
                effector.useOneWay = state; // Enable or disable one-way functionality
            }
            if (platform.TryGetComponent(out SpriteRenderer platformRenderer))
            {
                platformRenderer.color = state ? activatedColor : deactivatedColor;
            }
        }
    }

    void UpdateLeverState()
    {
        leverOn.SetActive(isActivated);
        leverOff.SetActive(!isActivated);
    }

    bool PlayerIsNearby()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Player"));
        return player != null;
    }
}
