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

    private LineRenderer lineRenderer;
    private readonly List<Vector3> laserPoints = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void FixedUpdate()
    {
        DrawLaser();
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
