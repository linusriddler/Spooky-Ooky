
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashDuration = 0.5f;

    [Range(0f, 1f)]
    public float flashStrength = 0.8f;

    private RawImage overlay;
    private Texture2D vignetteTexture;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        overlay = GetComponent<RawImage>();

        // Prevent the image from blocking UI buttons.
        overlay.raycastTarget = false;

        // Generate the red edges.
        CreateVignette();

        // Start completely invisible.
        overlay.color = new Color(1f, 1f, 1f, 0f);
    }

    private void CreateVignette()
    {
        int size = 128;

        vignetteTexture = new Texture2D(
            size, size, TextureFormat.RGBA32, false
        );

        // Loop through every pixel of the texture.
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Measure how far this pixel is from the center.
                float u = (float)x / (size - 1) - 0.5f;
                float v = (float)y / (size - 1) - 0.5f;

                float distance = new Vector2(u, v).magnitude;

                // Center is transparent, edges are red.
                float edgeStrength = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(0.22f, 0.7f, distance)
                );

                vignetteTexture.SetPixel(
                    x, y,
                    new Color(1f, 0f, 0f, edgeStrength)
                );
            }
        }

        vignetteTexture.wrapMode = TextureWrapMode.Clamp;
        vignetteTexture.filterMode = FilterMode.Bilinear;
        vignetteTexture.Apply();

        overlay.texture = vignetteTexture;
    }

    // PlayerHealth calls this whenever damage is taken.
    public void Flash()
    {
        // Restart the flash if the player is hit again.
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;

        while (timer < flashDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / Mathf.Max(0.01f, flashDuration)
            );

            // Fade from strong red to invisible.
            float alpha = Mathf.Lerp(
                flashStrength, 0f, progress
            );

            overlay.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        overlay.color = new Color(1f, 1f, 1f, 0f);
        flashCoroutine = null;
    }

    private void OnDestroy()
    {
        // Clean up the texture created by this script.
        if (vignetteTexture != null)
        {
            Destroy(vignetteTexture);
        }
    }
}
