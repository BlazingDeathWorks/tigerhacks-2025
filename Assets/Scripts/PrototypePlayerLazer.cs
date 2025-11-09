using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PrototypePlayerLazer : MonoBehaviour
{
    [Header("Laser Settings")]
    public Transform spawnPoint;           // Where the laser starts (e.g., gun tip)
    public float maxDistance = 50f;        // Max distance per segment
    public int maxBounces = 5;             // Number of reflections
    public LayerMask reflectionMask;       // Layers the laser can hit

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
    }

    void OnEnable()
    {
        // This is called when the weapon is switched to.
        // You might want to update UI here.
        // We won't reset ammo on switch, so it persists.
    }
    
    void OnDisable()
    {
        // Deactivate the laser when switching away from this weapon
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
            currentAmmo = (int)Mathf.Max(0, currentAmmo - ammoConsumptionRate * Time.deltaTime);
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
                // Add the hit point
                laserPoints.Add(hit.point);

                // Reflect the direction
                direction = Vector2.Reflect(direction, hit.normal).normalized;

                // Move start slightly away from surface to prevent infinite loops
                currentPos = hit.point + direction * 0.01f;
            }
            else
            {
                // No hit: go straight until max distance
                laserPoints.Add(currentPos + direction * maxDistance);
                break;
            }
        }

        // Apply to LineRenderer
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
    }
}