
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AZE.AdvancedFirstPerson;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 200f;
    public float currentHealth;

    [Header("Passive Healing")]
    public float secondsBeforeHealing = 4f;
    public float healingPerSecond = 40f;

    private float timeSinceLastDamage = 0f;
    private bool isDead = false;

    [Header("Health UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        currentHealth = maxHealth;

        UpdateHealthUI();

        // Hide the health bar and number at full health.
        SetHealthUIVisible(false);
    }

    private void Update()
    {
        // Don't heal if dead.
        if (isDead)
            return;

        // Hide the UI if health is already full.
        if (currentHealth >= maxHealth)
        {
            SetHealthUIVisible(false);
            return;
        }

        timeSinceLastDamage += Time.deltaTime;

        // Start healing after the delay.
        if (timeSinceLastDamage >= secondsBeforeHealing)
        {
            currentHealth = Mathf.Min(
                maxHealth,
                currentHealth + healingPerSecond * Time.deltaTime
            );

            UpdateHealthUI();

            // Hide the UI once health is completely restored.
            if (currentHealth >= maxHealth)
            {
                SetHealthUIVisible(false);
            }
        }
    }

    // Called by the enemy's melee attack.
    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        // Reset the healing countdown every time damage is taken.
        timeSinceLastDamage = 0f;

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        SetHealthUIVisible(true);
        UpdateHealthUI();
    }

    private void Die()
    {
        isDead = true;

        // Stop the player's movement.
        PlayerMovementStateMachine movement =
            GetComponent<PlayerMovementStateMachine>();

        if (movement != null)
            movement.enabled = false;

        Debug.Log("Player died!");
    }

    private void SetHealthUIVisible(bool visible)
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(visible);

        if (healthText != null)
            healthText.gameObject.SetActive(visible);
    }
}
