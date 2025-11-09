using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PrototypePlayerLazer : WeaponBase
{
    [Header("Laser Settings")]
    public Transform spawnPoint;           // Where the laser starts (e.g., gun tip)
    public float maxDistance = 50f;        // Max distance per segment
    public int maxBounces = 5;             // Number of reflections
    public LayerMask reflectionMask;       // Layers the laser can hit
    public Color laserColor = Color.green; // New: Public field for laser color

    [Header("Ammo Settings")]
    public int maxAmmo = 200;
    public int currentAmmo;
    public float ammoConsumptionRate = 40f; // Ammo consumed per second

    private LineRenderer lineRenderer;
    private readonly List<Vector3> laserPoints = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentAmmo = maxAmmo;

        // Apply the chosen laser color
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
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
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && currentAmmo > 0)
        {
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
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

        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
    }
}
