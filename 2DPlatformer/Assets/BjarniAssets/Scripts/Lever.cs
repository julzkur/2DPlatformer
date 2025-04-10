using Unity.Mathematics;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject[] platforms; // Assign the platforms affected by the lever
    public Color activatedColor = new Color(0.2f, 0.6f, 0.2f); // Dark green
    public Color deactivatedColor = new Color(0.6f, 0.2f, 0.2f); // Dark red
    public GameObject leverOn;
    public GameObject leverOff;
    public bool isActivated = false;
    public bool canChange = false;

    void Start()
    {
        print("Starting");
        UpdateLeverState();
    }

    void Update() 
    {
        if (canChange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                print("E");
                isActivated = !isActivated;
                TogglePlatforms(isActivated);
                UpdateLeverState();
            }
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

    void OnTriggerEnter2D (Collider2D collision)
    {
        canChange = true;
    }

    void OnTriggerExit2D (Collider2D collision)
    {
        canChange = false;
    }
}
