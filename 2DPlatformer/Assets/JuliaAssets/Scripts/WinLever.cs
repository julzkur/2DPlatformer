using Unity.Mathematics;
using UnityEngine;

public class WinLever : MonoBehaviour
{
    private Canvas winCanvas;
    public Color activatedColor = new Color(0.2f, 0.6f, 0.2f); // Dark green
    public Color deactivatedColor = new Color(0.6f, 0.2f, 0.2f); // Dark red
    public GameObject leverOn;
    public GameObject leverOff;
    public bool isActivated = false;
    public bool canChange = false;
    public LayerMask oneWayLayer;

    AudioManager audioManager;

    void Awake()
    {
        // Try to find the Canvas in the scene by name (assuming it's named "WinCanvas")
        winCanvas = GameObject.Find("WinCanvas")?.GetComponent<Canvas>();

        if (winCanvas == null)
        {
            Debug.LogError("WinCanvas not found in the scene.");
        }
        
        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
        }

        UpdateLeverState();
    }

    void Update() 
    {
        if (canChange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                print("E");
                audioManager.PlaySFX(audioManager.lever);
                isActivated = !isActivated;
                UpdateLeverState();
                if (isActivated)
                {
                    TriggerWinCanvas();
                }
            }
        }
    }

    // New method to show the WinCanvas
    void TriggerWinCanvas()
    {
        if (winCanvas != null)
        {
            winCanvas.gameObject.SetActive(true); // Show the win screen
        }
        else
        {
            Debug.LogWarning("WinCanvas not assigned in the inspector.");
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
