
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject gruntPrefab;
    public GameObject axeManPrefab;
    public GameObject wizardPrefab;
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    public int startingEnemyCount = 3;
    public int enemiesAddedPerWave = 2;
    public float timeBetweenSpawns = 1.5f;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Limits")]
    [Min(1)] public int maxAliveEnemies = 6;

    // Enemies that haven't spawned yet in the current wave.
    private int enemiesWaitingToSpawn = 0;

    [Header("Axe Man Settings")]
    public int axeManStartingWave = 3;
    public int axeMenAddedPerWave = 1;

    [Header("Wizard Settings")]
    public int wizardStartingWave = 4;
    public int wizardsAddedPerWave = 1;



    [Header("Current Wave")]
    public int currentWave = 1;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;

    [Header("Wave Announcement")]
    public TextMeshProUGUI waveAnnouncementText;
    public float announcementDuration = 2f;
    public float fadeDuration = 0.5f;

    // Track all spawned enemies.
    private List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        if (gruntPrefab == null ||
            axeManPrefab == null ||
            wizardPrefab == null)
        {
            Debug.LogError("WaveSpawner is missing an enemy prefab!");
            return;
        }

        // Check that spawn points exist.
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        StartCoroutine(WaveRoutine());
    }

    private void Update()
    {
        // Remove enemies that have been destroyed.
        aliveEnemies.RemoveAll(enemy => enemy == null);

        // Update wave number.
        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave;
        }

        // Update Enemies Left text.
        if (enemiesLeftText != null)
        {
            int totalRemaining =
                aliveEnemies.Count + enemiesWaitingToSpawn;

            enemiesLeftText.text =
                totalRemaining + " ENEMIES LEFT";
        }
    }


    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            // Calculate total enemies in this wave.
            int totalEnemies = startingEnemyCount +
                (currentWave - 1) * enemiesAddedPerWave;

            enemiesWaitingToSpawn = totalEnemies;   

            // Calculate how many Wizards to spawn.
            int wizardCount = 0;

            if (currentWave >= wizardStartingWave)
            {
                wizardCount =
                    (currentWave - wizardStartingWave + 1)
                    * wizardsAddedPerWave;
            }

            wizardCount = Mathf.Clamp(
                wizardCount, 0, totalEnemies
            );

            // Calculate how many Axe Men to spawn.
            int axeMenCount = 0;

            if (currentWave >= axeManStartingWave)
            {
                axeMenCount =
                    (currentWave - axeManStartingWave + 1)
                    * axeMenAddedPerWave;
            }

            // Leave enough room for Wizards.
            axeMenCount = Mathf.Clamp(
                axeMenCount,
                0,
                totalEnemies - wizardCount
            );

            // Everyone else is a Grunt.
            int gruntCount =
                totalEnemies - axeMenCount - wizardCount;

            Debug.Log(
                "Wave " + currentWave +
                ": " + gruntCount + " Grunts, " +
                axeMenCount + " Axe Men, " +
                wizardCount + " Wizards"
            );

            // Show the wave announcement.
            StartCoroutine(ShowWaveAnnouncement());

            // Track which enemies still need to spawn.
            int gruntsLeft = gruntCount;
            int axeMenLeft = axeMenCount;
            int wizardsLeft = wizardCount;

            // Spawn all enemies in randomized order.
            for (int i = 0; i < totalEnemies; i++)
            {

                // Wait until there's room for another enemy.
                while (true)
                {
                    // Remove defeated enemies from our tracking list.
                    aliveEnemies.RemoveAll(enemy => enemy == null);

                    // Continue spawning when we're below the limit.
                    if (aliveEnemies.Count < Mathf.Max(1, maxAliveEnemies))
                        break;

                    // Wait one frame before checking again.
                    yield return null;
                }

                int remaining =
                    gruntsLeft + axeMenLeft + wizardsLeft;

                int randomChoice = Random.Range(0, remaining);

                GameObject chosenPrefab;

                if (randomChoice < gruntsLeft)
                {
                    chosenPrefab = gruntPrefab;
                    gruntsLeft--;
                }
                else if (randomChoice < gruntsLeft + axeMenLeft)
                {
                    chosenPrefab = axeManPrefab;
                    axeMenLeft--;
                }
                else
                {
                    chosenPrefab = wizardPrefab;
                    wizardsLeft--;
                }

                if (!SpawnEnemy(chosenPrefab))
                {
                    yield break;
                }

                // One fewer enemy is waiting to spawn.
                enemiesWaitingToSpawn--;

                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            // Wait until every enemy is defeated.
            while (aliveEnemies.Count > 0)
            {
                aliveEnemies.RemoveAll(enemy => enemy == null);
                yield return null;
            }

            Debug.Log("Wave " + currentWave + " completed!");

            // Break before starting the next wave.
            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
        }
    }

    private bool SpawnEnemy(GameObject enemyPrefab)
    {
        // Pick a random starting spawn point.
        int randomIndex = Random.Range(0, spawnPoints.Length);

        // Look for a valid position on the NavMesh.
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int index = (randomIndex + i) % spawnPoints.Length;
            Transform point = spawnPoints[index];

            if (point == null)
                continue;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                point.position,
                out hit,
                1.5f,
                NavMesh.AllAreas))
            {
                GameObject newEnemy = Instantiate(
                    enemyPrefab,
                    hit.position,
                    point.rotation
                );

                // Add the newly spawned enemy to our list.
                aliveEnemies.Add(newEnemy);

                return true;
            }
        }

        Debug.LogError("No valid NavMesh spawn position found!");
        return false;
    }

    private IEnumerator ShowWaveAnnouncement()
    {
        if (waveAnnouncementText == null)
            yield break;

        waveAnnouncementText.text = "WAVE " + currentWave;
        waveAnnouncementText.alpha = 1f;
        waveAnnouncementText.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < announcementDuration)
        {
            timer += Time.deltaTime;

            float fadeStart = Mathf.Max(
                0f,
                announcementDuration - fadeDuration
            );

            if (timer >= fadeStart)
            {
                float remainingTime =
                    announcementDuration - timer;

                waveAnnouncementText.alpha = Mathf.Clamp01(
                    remainingTime /
                    Mathf.Max(0.01f,
                        announcementDuration - fadeStart)
                );
            }

            yield return null;
        }

        waveAnnouncementText.gameObject.SetActive(false);
    }
}
