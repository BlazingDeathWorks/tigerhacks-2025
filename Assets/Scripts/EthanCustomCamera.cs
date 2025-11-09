using System.Diagnostics;
using UnityEngine;

public class EthanCustomCamera : MonoBehaviour
{
    int ROOM_SIZE = 16 * 4;
    public int transitionStatus = 0;
    int targetX = 0;
    int targetY = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void runCameraTransition(int direction)
    {
        transitionStatus = 1;
        switch (direction)
        {
            case 0:
                targetY -= ROOM_SIZE;
                break;
            case 1:
                targetY += ROOM_SIZE;
                break;
            case 2:
                targetX += ROOM_SIZE;
                break;
            case 3:
                targetX -= ROOM_SIZE;
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transitionStatus == 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetX, targetY, -10), Time.deltaTime * 85.0f);
            if (Vector3.Distance(transform.position, new Vector3(targetX, targetY, -10)) < .1f) 
            {
                UnityEngine.Debug.Log("Camera reached end position");
                transform.position = new Vector3(targetX, targetY, -10);
                transitionStatus = 2;
                UnityEngine.Debug.Log("Transition Status was Updated");
            }
        }
    }
}
