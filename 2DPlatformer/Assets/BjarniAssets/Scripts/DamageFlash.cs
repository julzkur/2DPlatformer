using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;

    public Image flashImage;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1, 0, 0, 0.3f); // Semi-transparent red

    private void Awake()
    {
        Instance = this;
    }

    public void Flash()
    {
        StopAllCoroutines(); // Stops previous flash if still running
        StartCoroutine(DoFlash());
    }

    private System.Collections.IEnumerator DoFlash()
    {
        flashImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            flashImage.color = Color.Lerp(flashColor, Color.clear, elapsed / flashDuration);
            yield return null;
        }

        flashImage.color = Color.clear;
    }
}
