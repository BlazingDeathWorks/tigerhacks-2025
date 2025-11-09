using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complex multi-phase boss bullet hell controller.
/// Each phase contains multiple bullet stacks (patterns) with positions relative to the boss.
/// </summary>
public class BossBulletPatternController : MonoBehaviour, IObjectPooler<SmallEnemyBullet>
{
    [Header("References")]
    [SerializeField] private Transform bulletSpawnParent; // Usually the boss transform
    [SerializeField] private SmallEnemyBullet bulletPrefab;
    [SerializeField] private float fireDelayBetweenStacks = 0.5f;
    [SerializeField] private float fireDelayBetweenPhases = 2f;

    [Header("Debug")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private bool visualizeGizmos = true;
    
    private List<List<Vector2>> _phase1Patterns;
    private List<List<Vector2>> _phase2Patterns;
    private List<List<Vector2>> _phase3Patterns;
    private List<List<Vector2>> _phase4Patterns; // Spiral Madness
    private List<List<Vector2>> _phase5Patterns; // Insane Spiral
    private List<List<Vector2>> _phase6Patterns; // Fast Burst
    private List<List<Vector2>> _phase7Patterns; // Flower Bloom
    private List<List<Vector2>> _phase8Patterns; // Final Chaos
    private int _currentPhase = 0;
    public SmallEnemyBullet Prefab => bulletPrefab;
    public Queue<SmallEnemyBullet> Pool { get; } = new Queue<SmallEnemyBullet>();
    
    // Variables for object pooling
    private Vector3 _currentBulletPosition;
    private Vector3 _currentBulletDirection;


    private void Awake()
    {
        BuildPatterns();
    }

    private void Start()
    {
        if (startAutomatically)
            StartCoroutine(RunPhases());
    }

    public void OnPooled(SmallEnemyBullet instance)
    {
        instance.transform.position = _currentBulletPosition;
        instance.transform.up = _currentBulletDirection; // Aim outward
        instance.gameObject.SetActive(true);
    }

    private IEnumerator RunPhases()
    {
        yield return new WaitForSeconds(1f); // Small intro delay

        // Create array of all phase patterns for easy looping
        var allPhases = new List<List<Vector2>>[] 
        { 
            _phase1Patterns, _phase2Patterns, _phase3Patterns, _phase4Patterns, 
            _phase5Patterns, _phase6Patterns, _phase7Patterns, _phase8Patterns 
        };

        // Loop through phases infinitely
        while (true)
        {
            for (int phaseIndex = 0; phaseIndex < allPhases.Length; phaseIndex++)
            {
                _currentPhase = phaseIndex + 1;
                yield return StartCoroutine(FirePhase(allPhases[phaseIndex]));
                
                // Add delay between phases (except before looping back to phase 1)
                if (phaseIndex < allPhases.Length - 1)
                {
                    yield return new WaitForSeconds(fireDelayBetweenPhases);
                }
                else
                {
                    // Longer delay before looping back to phase 1
                    yield return new WaitForSeconds(fireDelayBetweenPhases * 2f);
                }
            }
        }
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
            // Store the position for use in OnPooled
            _currentBulletPosition = bulletSpawnParent.TransformPoint(pos);
            _currentBulletDirection = (_currentBulletPosition - transform.position).normalized;
            
            // Use object pooling instead of instantiation
            ObjectPool.Pool(this);
        }
    }

    /// <summary>
    /// Builds all bullet stack pattern lists.
    /// </summary>
    private void BuildPatterns()
    {
        // === PHASE 1: Simple Circular Spray ===
        _phase1Patterns = new List<List<Vector2>>();
        int ringCount = 2;
        int bulletsPerRing = 6; // Much fewer bullets
        float ringSpacing = 2.5f; // More spacing

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
        int spiralArms = 3; // Fewer arms
        for (int arm = 0; arm < spiralArms; arm++)
        {
            List<Vector2> spiral = new List<Vector2>();
            for (int j = 0; j < 5; j++) // Fewer bullets per spiral
            {
                float angle = (arm * 120f + j * 30f) * Mathf.Deg2Rad; // More spacing
                float radius = 1f + j * 0.6f; // Larger spacing
                spiral.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase2Patterns.Add(spiral);
        }

        // === PHASE 3: Simple Cross Pattern ===
        _phase3Patterns = new List<List<Vector2>>();
        for (int layer = 0; layer < 2; layer++) // Fewer layers
        {
            List<Vector2> cluster = new List<Vector2>();
            for (int i = -2; i <= 2; i++) // Fewer bullets
            {
                Vector2 pos = new Vector2(i * 1.8f, Mathf.Sin(i + layer) * 2f); // More spacing
                cluster.Add(pos);
            }
            _phase3Patterns.Add(cluster);
        }

        // Add simple diagonal pattern
        List<Vector2> diagonals = new List<Vector2>();
        for (int i = -2; i <= 2; i++) // Fewer diagonals
        {
            diagonals.Add(new Vector2(i * 1.5f, i * 1.5f)); // Down-right diagonal
            diagonals.Add(new Vector2(i * 1.5f, -i * 1.5f)); // Up-right diagonal
        }
        _phase3Patterns.Add(diagonals);

        // === PHASE 4: Gentle Spiral ===
        _phase4Patterns = new List<List<Vector2>>();
        int spiralStacks = 4; // Fewer spirals
        for (int stack = 0; stack < spiralStacks; stack++)
        {
            List<Vector2> spiralStack = new List<Vector2>();
            for (int i = 0; i < 8; i++) // Fewer bullets per spiral
            {
                float angle = (stack * 90f + i * 45f) * Mathf.Deg2Rad; // More spacing
                float radius = 1.5f + i * 0.4f; // Larger spacing
                spiralStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase4Patterns.Add(spiralStack);
        }

        // === PHASE 5: Wavy Spiral ===
        _phase5Patterns = new List<List<Vector2>>();
        int insaneSpirals = 4; // Much fewer spirals
        for (int spiral = 0; spiral < insaneSpirals; spiral++)
        {
            List<Vector2> insaneStack = new List<Vector2>();
            for (int i = 0; i < 10; i++) // Fewer bullets
            {
                float angle = (spiral * 90f + i * 36f) * Mathf.Deg2Rad; // More spacing
                float radius = 1f + i * 0.3f;
                // Add gentle wave modulation
                radius += Mathf.Sin(i * 0.8f) * 0.3f; // Gentler modulation
                insaneStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase5Patterns.Add(insaneStack);
        }

        // === PHASE 6: Gentle Burst ===
        _phase6Patterns = new List<List<Vector2>>();
        int burstCount = 8; // Fewer bursts
        for (int burst = 0; burst < burstCount; burst++)
        {
            List<Vector2> burstStack = new List<Vector2>();
            float baseAngle = burst * 45f * Mathf.Deg2Rad; // More spacing between bursts
            
            // Small cluster of bullets in one direction
            for (int i = 0; i < 4; i++) // Fewer bullets per burst
            {
                float angleVariation = (i - 1.5f) * 10f * Mathf.Deg2Rad; // More spread
                float radius = 2.5f + i * 0.3f; // More spacing
                float finalAngle = baseAngle + angleVariation;
                burstStack.Add(new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * radius);
            }
            _phase6Patterns.Add(burstStack);
        }

        // === PHASE 7: Simple Flower ===
        _phase7Patterns = new List<List<Vector2>>();
        int petalCount = 6; // Fewer petals
        for (int petal = 0; petal < petalCount; petal++)
        {
            List<Vector2> petalStack = new List<Vector2>();
            float petalAngle = petal * 60f * Mathf.Deg2Rad; // More spacing
            
            // Create simple petal shape
            for (int i = 0; i < 6; i++) // Fewer bullets per petal
            {
                float t = i / 5f; // 0 to 1
                float radius = Mathf.Sin(t * Mathf.PI) * 3f; // Smaller petals
                float angle = petalAngle + (t - 0.5f) * 0.2f; // Less curve
                petalStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase7Patterns.Add(petalStack);
        }

        // === PHASE 8: FINAL CHAOS - ULTIMATE MAYHEM ===
        _phase8Patterns = new List<List<Vector2>>();
        
        // INSANE SPIRALS - Multiple overlapping super-fast spirals
        int megaSpiralCount = 20;
        for (int spiral = 0; spiral < megaSpiralCount; spiral++)
        {
            List<Vector2> megaSpiralStack = new List<Vector2>();
            for (int i = 0; i < 30; i++) // WAY more bullets per spiral
            {
                float angle = (spiral * 18f + i * 12f) * Mathf.Deg2Rad; // Super tight spiral
                float radius = 0.3f + i * 0.15f;
                // Add chaos modulation
                radius += Mathf.Sin(i * 0.8f + spiral) * 0.4f;
                radius += Mathf.Cos(i * 1.2f + spiral * 0.5f) * 0.3f;
                megaSpiralStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase8Patterns.Add(megaSpiralStack);
        }
        
        // EXPLOSIVE RINGS - Dense concentric rings
        int explosiveRings = 15;
        for (int ring = 0; ring < explosiveRings; ring++)
        {
            List<Vector2> explosiveRingStack = new List<Vector2>();
            int bulletsInRing = 24 + ring * 4; // Increasing density
            float ringRadius = 1f + ring * 0.4f;
            
            for (int i = 0; i < bulletsInRing; i++)
            {
                float angle = i * Mathf.PI * 2 / bulletsInRing;
                // Add chaotic variations
                angle += Mathf.Sin(ring * 0.7f + i * 0.3f) * 0.2f;
                ringRadius += Mathf.Cos(i * 0.5f + ring) * 0.2f;
                explosiveRingStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius);
            }
            _phase8Patterns.Add(explosiveRingStack);
        }
        
        // RANDOM CHAOS CLUSTERS - Pure mayhem
        int chaosStacks = 40; // DOUBLED chaos stacks
        for (int chaos = 0; chaos < chaosStacks; chaos++)
        {
            List<Vector2> chaosStack = new List<Vector2>();
            
            System.Random rand = new System.Random(chaos + 1000); // Different seed for more chaos
            int bulletsInStack = rand.Next(15, 35); // WAY more bullets per stack
            
            for (int i = 0; i < bulletsInStack; i++)
            {
                float angle = (float)rand.NextDouble() * Mathf.PI * 2;
                float radius = 0.5f + (float)rand.NextDouble() * 5f; // Larger spread
                
                // EXTREME chaos modulation
                if (i % 2 == 0) radius *= 1.8f;
                if (i % 5 == 0) radius *= 0.3f; // Some very close bullets
                if (i % 7 == 0) radius *= 2.5f; // Some VERY far bullets
                
                // Add sine/cosine chaos
                radius += Mathf.Sin(i * 1.5f + chaos) * 0.8f;
                angle += Mathf.Cos(i * 0.7f + chaos) * 0.5f;
                
                chaosStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            _phase8Patterns.Add(chaosStack);
        }
        
        // DEATH CROSS PATTERNS - X-shaped death
        int crossPatterns = 12;
        for (int cross = 0; cross < crossPatterns; cross++)
        {
            List<Vector2> crossStack = new List<Vector2>();
            float baseAngle = cross * 30f * Mathf.Deg2Rad;
            
            // Four arms of the cross
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = baseAngle + arm * 90f * Mathf.Deg2Rad;
                for (int i = 0; i < 12; i++)
                {
                    float radius = 0.5f + i * 0.4f;
                    // Add some spread to each arm
                    float spreadAngle = armAngle + (i % 3 - 1) * 0.1f;
                    crossStack.Add(new Vector2(Mathf.Cos(spreadAngle), Mathf.Sin(spreadAngle)) * radius);
                }
            }
            _phase8Patterns.Add(crossStack);
        }
        
        // FINAL APOCALYPSE WAVE - The ultimate chaos
        List<Vector2> apocalypseStack = new List<Vector2>();
        for (int i = 0; i < 200; i++) // 200 BULLETS IN ONE STACK!
        {
            float angle = i * 0.31f; // Prime number for non-repeating pattern
            float radius = 1f + (i % 20) * 0.3f;
            
            // Triple modulation for maximum chaos
            radius += Mathf.Sin(i * 0.1f) * 1.2f;
            radius += Mathf.Cos(i * 0.07f) * 0.8f;
            radius += Mathf.Sin(i * 0.13f) * 0.6f;
            
            apocalypseStack.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        _phase8Patterns.Add(apocalypseStack);
    }

    private void OnDrawGizmosSelected()
    {
        if (!visualizeGizmos || bulletSpawnParent == null) return;

        Gizmos.color = _currentPhase switch
        {
            1 => Color.green,
            2 => Color.cyan,
            3 => Color.magenta,
            4 => Color.yellow,
            5 => Color.red,
            6 => Color.blue,
            7 => new Color(1f, 0.5f, 0f), // Orange
            8 => Color.black,
            _ => Color.white
        };

        if (_phase1Patterns == null) return;
        var allPhases = new[] { _phase1Patterns, _phase2Patterns, _phase3Patterns, _phase4Patterns, _phase5Patterns, _phase6Patterns, _phase7Patterns, _phase8Patterns };
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
