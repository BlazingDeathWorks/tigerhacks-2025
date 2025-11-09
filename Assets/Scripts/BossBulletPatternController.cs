using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complex multi-phase boss bullet hell controller.
/// Each phase contains multiple bullet stacks (patterns) with positions relative to the boss.
/// </summary>
public class BossBulletPatternController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bulletSpawnParent; // Usually the boss transform
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireDelayBetweenStacks = 0.5f;
    [SerializeField] private float fireDelayBetweenPhases = 2f;

    [Header("Debug")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private bool visualizeGizmos = true;
    
    private List<List<Vector2>> _phase1Patterns;
    private List<List<Vector2>> _phase2Patterns;
    private List<List<Vector2>> _phase3Patterns;
    private int _currentPhase = 0;

    private void Awake()
    {
        BuildPatterns();
    }

    private void Start()
    {
        if (startAutomatically)
            StartCoroutine(RunPhases());
    }

    private IEnumerator RunPhases()
    {
        yield return new WaitForSeconds(1f); // Small intro delay

        _currentPhase = 1;
        yield return StartCoroutine(FirePhase(_phase1Patterns));

        _currentPhase = 2;
        yield return new WaitForSeconds(fireDelayBetweenPhases);
        yield return StartCoroutine(FirePhase(_phase2Patterns));

        _currentPhase = 3;
        yield return new WaitForSeconds(fireDelayBetweenPhases);
        yield return StartCoroutine(FirePhase(_phase3Patterns));
    }

    private IEnumerator FirePhase(List<List<Vector2>> phase)
    {
        foreach (var stack in phase)
        {
            FireStack(stack);
            yield return new WaitForSeconds(fireDelayBetweenStacks);
        }
    }

    private void FireStack(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            Vector3 worldPos = bulletSpawnParent.TransformPoint(pos);
            GameObject b = Instantiate(bulletPrefab, worldPos, Quaternion.identity);
            b.transform.up = (worldPos - transform.position).normalized; // Aim outward
        }
    }

    /// <summary>
    /// Builds all bullet stack pattern lists.
    /// </summary>
    private void BuildPatterns()
    {
        // === PHASE 1: Simple Circular Spray ===
        _phase1Patterns = new List<List<Vector2>>();
        int ringCount = 3;
        int bulletsPerRing = 12;
        float ringSpacing = 1.5f;

        for (int r = 1; r <= ringCount; r++)
        {
            List<Vector2> ring = new List<Vector2>();
            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = i * Mathf.PI * 2 / bulletsPerRing;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (r * ringSpacing);
                ring.Add(pos);
            }
            _phase1Patterns.Add(ring);
        }

        // === PHASE 2: Spiraling Tri-Ring + Offset Layer ===
        _phase2Patterns = new List<List<Vector2>>();
        int spiralArms = 6;
        for (int arm = 0; arm < spiralArms; arm++)
        {
            List<Vector2> spiral = new List<Vector2>();
            for (int j = 0; j < 10; j++)
            {
                float angle = (arm * 60f + j * 20f) * Mathf.Deg2Rad;
                float radius = 0.5f + j * 0.4f;
                spiral.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase2Patterns.Add(spiral);
        }

        // === PHASE 3: Chaotic Crossfire Pattern ===
        _phase3Patterns = new List<List<Vector2>>();
        for (int layer = 0; layer < 5; layer++)
        {
            List<Vector2> cluster = new List<Vector2>();
            for (int i = -3; i <= 3; i++)
            {
                Vector2 pos = new Vector2(i * 1.2f, Mathf.Sin(i + layer) * 3f);
                cluster.Add(pos);
            }
            _phase3Patterns.Add(cluster);
        }

        // Add diagonal wings
        List<Vector2> diagonals = new List<Vector2>();
        for (int i = -4; i <= 4; i++)
        {
            diagonals.Add(new Vector2(i, i)); // Down-right diagonal
            diagonals.Add(new Vector2(i, -i)); // Up-right diagonal
        }
        _phase3Patterns.Add(diagonals);
    }

    private void OnDrawGizmosSelected()
    {
        if (!visualizeGizmos || bulletSpawnParent == null) return;

        Gizmos.color = _currentPhase switch
        {
            1 => Color.green,
            2 => Color.cyan,
            3 => Color.magenta,
            _ => Color.white
        };

        if (_phase1Patterns == null) return;
        var allPhases = new[] { _phase1Patterns, _phase2Patterns, _phase3Patterns };
        foreach (var phase in allPhases)
        {
            if (phase == null) continue;
            foreach (var stack in phase)
            {
                foreach (var p in stack)
                {
                    Gizmos.DrawSphere(bulletSpawnParent.TransformPoint(p), 0.1f);
                }
            }
        }
    }
}
