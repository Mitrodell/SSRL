using UnityEngine;
using System.Collections.Generic;

public sealed class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Enemy Prefabs (root has Enemy child-class)")]
    [SerializeField] private GameObject meleePrefab;
    [SerializeField] private GameObject rangedPrefab;
    [SerializeField] private GameObject tankPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int baseCount = 3;
    [SerializeField] private int maxCount = 25;

    [Header("Scaling")]
    [SerializeField] private float hpPerWave = 0.08f;
    [SerializeField] private float dmgPerWave = 0.05f;

    [Header("Anti-overlap")]
    [SerializeField] private float spawnScatterRadius = 2.0f;
    [SerializeField] private float spawnCheckRadius = 0.8f;
    [SerializeField] private int spawnTries = 12;
    [SerializeField] private LayerMask enemyBlockMask;

    private readonly List<Enemy> alive = new List<Enemy>(64);

    public int AliveCount
    {
        get { ClearNulls(); return alive.Count; }
    }

    public void SpawnWave(int wave)
    {
        if (!ValidateSetup()) return;

        ClearNulls();

        int count = Mathf.Clamp(baseCount + wave, baseCount, maxCount);

        float hpMul = 1f + wave * hpPerWave;
        float dmgMul = 1f + wave * dmgPerWave;

        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = PickPrefab(wave);

            Vector3 pos = GetSpawnPosition(sp.position);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning("[EnemySpawner] Spawned prefab without Enemy component. Destroying spawned object.");
                Destroy(go);
                continue;
            }

            enemy.ApplyWaveScaling(hpMul, dmgMul);
            alive.Add(enemy);
        }
    }

    public bool IsWaveCleared()
    {
        ClearNulls();
        return alive.Count == 0;
    }

    private Vector3 GetSpawnPosition(Vector3 center)
    {
        if (TryGetFreeSpawnPos(center, out Vector3 free))
            return free;
        return center;
    }

    private bool TryGetFreeSpawnPos(Vector3 center, out Vector3 pos)
    {
        if (enemyBlockMask.value == 0)
        {
            pos = center;
            return true;
        }

        for (int i = 0; i < Mathf.Max(1, spawnTries); i++)
        {
            Vector2 r = Random.insideUnitCircle * Mathf.Max(0f, spawnScatterRadius);
            Vector3 candidate = center + new Vector3(r.x, 0f, r.y);

            bool blocked = Physics.CheckSphere(
                candidate,
                Mathf.Max(0.01f, spawnCheckRadius),
                enemyBlockMask,
                QueryTriggerInteraction.Ignore
            );

            if (!blocked)
            {
                pos = candidate;
                return true;
            }
        }

        pos = center;
        return false;
    }

    private GameObject PickPrefab(int wave)
    {
        float r = Random.value;

        if (wave < 3)
            return meleePrefab;

        if (r < 0.55f) return meleePrefab;
        if (r < 0.85f) return rangedPrefab;
        return tankPrefab;
    }

    private void ClearNulls()
    {
        alive.RemoveAll(e => e == null);
    }

    private bool ValidateSetup()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No spawn points assigned.");
            return false;
        }

        if (meleePrefab == null || rangedPrefab == null || tankPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] One or more enemy prefabs are not assigned.");
            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        baseCount = Mathf.Max(1, baseCount);
        maxCount = Mathf.Max(baseCount, maxCount);

        hpPerWave = Mathf.Max(0f, hpPerWave);
        dmgPerWave = Mathf.Max(0f, dmgPerWave);

        spawnScatterRadius = Mathf.Max(0f, spawnScatterRadius);
        spawnCheckRadius = Mathf.Max(0.01f, spawnCheckRadius);
        spawnTries = Mathf.Clamp(spawnTries, 1, 50);
    }
#endif
}
