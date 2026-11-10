
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

    [Header("Damage Feedback")]
    public DamageFlash damageFlash;

    [Header("Game Over")]
    public GameOverUI gameOverUI;

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

        // Reduce player health.
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        // Reset the regeneration timer.
        timeSinceLastDamage = 0f;

        // Display the health bar.
        SetHealthUIVisible(true);

        // Update the health values.
        UpdateHealthUI();

        // Play the red damage flash.
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        // Check if the player has died.
        if (currentHealth <= 0f)
        {
            Die();
        }
    }



    private void UpdateHealthUI()
    {
        // Update the green health bar.
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Update the health number.
        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }



    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Disable player movement.
        PlayerMovementStateMachine movement =
            GetComponent<PlayerMovementStateMachine>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        // Disable player movement input.
        PlayerInputHandler inputHandler =
            GetComponent<PlayerInputHandler>();

        if (inputHandler != null)
        {
            inputHandler.enabled = false;
        }

        // Hide damage flash so it doesn't cover the menu.
        if (damageFlash != null)
        {
            damageFlash.gameObject.SetActive(false);
        }

        // Display the Game Over screen.
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }

        Debug.Log("Player died!");
    }


    private void SetHealthUIVisible(bool visible)
    {
        if (healthSlider != null &&
            healthSlider.gameObject.activeSelf != visible)
        {
            healthSlider.gameObject.SetActive(visible);
        }

        if (healthText != null &&
            healthText.gameObject.activeSelf != visible)
        {
            healthText.gameObject.SetActive(visible);
        }
    }
}
