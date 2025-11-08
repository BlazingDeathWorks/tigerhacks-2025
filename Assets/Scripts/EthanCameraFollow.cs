using UnityEngine;

public class EthanCameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Vector3 offset = new Vector3(0, 0, -100);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + offset;
    }
}
