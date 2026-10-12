

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SpectralSlashAbility : MonoBehaviour
{
    [Header("References")]
    public Transform viewCamera;
    public GameObject spectralBladePrefab;

    [Header("Blade Position")]
    // Keep the sword handle near the bottom-right of the screen.
    public Vector3 bladePosition =
        new Vector3(0.4f, -0.8f, 1.2f);

    [Header("Animation Settings")]
    public float summonDuration = 0.10f;
    public float swingDuration = 0.35f;
    public float vanishDuration = 0.12f;

    // Sword rotates around its handle.
    public float startAngle = -60f;
    public float endAngle = 65f;

    [Header("Cooldown")]
    public float cooldown = 2f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private void Update()
    {
        // Press F to swing.
        if (Keyboard.current == null ||
            !Keyboard.current.fKey.wasPressedThisFrame)
            return;

        // Don't attack during a pause or cooldown.
        if (Time.timeScale == 0f ||
            isAttacking ||
            Time.time < nextAttackTime)
            return;

        // Make sure the references exist.
        if (viewCamera == null ||
            spectralBladePrefab == null)
            return;

        nextAttackTime = Time.time + cooldown;

        StartCoroutine(SlashAnimation());
    }

    private IEnumerator SlashAnimation()
    {
        isAttacking = true;

        // Create a sword attached to the camera.
        GameObject sword = Instantiate(
            spectralBladePrefab,
            viewCamera
        );

        Transform swordTransform = sword.transform;

        // The handle stays in this position.
        swordTransform.localPosition = bladePosition;

        // Set starting rotation.
        swordTransform.localRotation =
            Quaternion.Euler(0f, 0f, startAngle);

        // Start invisible (scaled down).
        swordTransform.localScale = Vector3.zero;

        // Find all Trail Renderers on the sword.
        TrailRenderer[] trails =
            sword.GetComponentsInChildren<TrailRenderer>();

        // No trail during summoning.
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = false;
            trail.Clear();
        }

        // -------------------------
        // PHASE 1: SUMMON
        // -------------------------

        float timer = 0f;

        while (timer < summonDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / Mathf.Max(0.001f, summonDuration)
            );

            // Grow the sword into existence.
            swordTransform.localScale =
                Vector3.one * progress;

            yield return null;
        }

        swordTransform.localScale = Vector3.one;

        // -------------------------
        // PHASE 2: SWING
        // -------------------------

        // Start generating the magical trail.
        foreach (TrailRenderer trail in trails)
        {
            trail.Clear();
            trail.emitting = true;
        }

        timer = 0f;

        while (timer < swingDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / Mathf.Max(0.001f, swingDuration)
            );

            // Smoothly accelerate and decelerate.
            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            // Rotate the sword around its handle.
            float angle = Mathf.Lerp(
                startAngle,
                endAngle,
                smoothProgress
            );

            swordTransform.localRotation =
                Quaternion.Euler(0f, 0f, angle);

            // IMPORTANT:
            // We do NOT move the sword's position.
            // This creates the invisible-hand swing.

            yield return null;
        }

        // Finish at the exact ending angle.
        swordTransform.localRotation =
            Quaternion.Euler(0f, 0f, endAngle);

        // Stop generating new trail segments.
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = false;
        }

        // -------------------------
        // PHASE 3: VANISH
        // -------------------------

        timer = 0f;

        while (timer < vanishDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / Mathf.Max(0.001f, vanishDuration)
            );

            // Shrink the sword until it disappears.
            swordTransform.localScale =
                Vector3.one * (1f - progress);

            yield return null;
        }

        swordTransform.localScale = Vector3.zero;

        // Let the magical trail fade naturally.
        Destroy(sword, 0.25f);

        isAttacking = false;
    }
}
