using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class PrototypePlayerLazer : WeaponBase
{
    [Header("Laser Settings")]
    public Transform spawnPoint;           // Where the laser starts (e.g., gun tip)
    public float maxDistance = 50f;        // Max distance per segment
    public int maxBounces = 5;             // Number of reflections
    public LayerMask reflectionMask;       // Layers the laser can hit
    public Color laserColor = Color.green; // Public field for laser color

    [Header("Ammo Settings")]
    public int maxAmmo = 200;
    public int currentAmmo;
    public float ammoConsumptionRate = 40f; // Ammo consumed per second

    private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D edgeCollider;
    private readonly List<Vector3> laserPoints = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentAmmo = maxAmmo;

        // Apply the chosen laser color
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
        lineRenderer.useWorldSpace = true;
    }

    void OnEnable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(currentAmmo);
        }
    }

    void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        if (edgeCollider != null)
        {
            edgeCollider.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && currentAmmo > 0)
        {
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
                edgeCollider.enabled = true;
            }

            DrawLaser();

            int oldAmmo = currentAmmo;
            currentAmmo = (int)Mathf.Max(0, currentAmmo - ammoConsumptionRate * Time.deltaTime);

            if (currentAmmo != oldAmmo && UIManager.Instance != null)
            {
                UIManager.Instance.UpdateAmmo(currentAmmo);
            }
        }
        else
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                edgeCollider.enabled = false;
            }
        }
    }

    void DrawLaser()
    {
        if (!spawnPoint) return;

        laserPoints.Clear();

        Vector2 currentPos = spawnPoint.position;
        Vector2 direction = spawnPoint.up; // Laser direction is "up" in 2D

        laserPoints.Add(spawnPoint.position);

        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, maxDistance, reflectionMask);

            if (hit.collider != null)
            {
                laserPoints.Add(hit.point);
                direction = Vector2.Reflect(direction, hit.normal).normalized;
                currentPos = hit.point + direction * 0.01f;
            }
            else
            {
                laserPoints.Add(currentPos + direction * maxDistance);
                break;
            }
        }

        // Update LineRenderer
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());

        // Update EdgeCollider
        UpdateEdgeCollider();
    }

    void UpdateEdgeCollider()
    {
        if (laserPoints.Count < 2)
        {
            edgeCollider.enabled = false;
            return;
        }

        // Convert world positions to collider-local positions
        Vector2[] colliderPoints = new Vector2[laserPoints.Count];
        for (int i = 0; i < laserPoints.Count; i++)
        {
            Vector3 worldPoint = laserPoints[i];
            Vector3 localPoint = edgeCollider.transform.InverseTransformPoint(worldPoint);
            colliderPoints[i] = new Vector2(localPoint.x, localPoint.y);
        }

        edgeCollider.points = colliderPoints;
        edgeCollider.enabled = true;
    }
}
