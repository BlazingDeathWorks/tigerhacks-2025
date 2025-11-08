using System;
using System.Collections.Generic;
using UnityEngine;

public class EthanRoom : MonoBehaviour
{
    [SerializeField] private Sprite[] backgrounds;

    public GameObject leftRoom;
    public GameObject rightRoom;
    public GameObject topRoom;
    public GameObject bottomRoom;
    
    public void Initialize(Dictionary<Vector2Int, GameObject> rooms, Vector2Int location, int ROOM_SIZE)
    {
        //Make sure all corners are properly set
        Vector2Int topLocation = location + new Vector2Int(0, ROOM_SIZE);
        if (rooms.ContainsKey(topLocation)) //if there is a room above
        {
            topRoom = rooms[topLocation];
            topRoom.GetComponent<EthanRoom>().bottomRoom = gameObject;
        }

        Vector2Int bottomLocation = location + new Vector2Int(0, -ROOM_SIZE);
        if (rooms.ContainsKey(bottomLocation)) //if there is a room above
        {
            bottomRoom = rooms[bottomLocation];
            bottomRoom.GetComponent<EthanRoom>().topRoom = gameObject;
        }

        Vector2Int leftLocation = location + new Vector2Int(-ROOM_SIZE, 0);
        if (rooms.ContainsKey(leftLocation)) //if there is a room above
        {
            leftRoom = rooms[leftLocation];
            leftRoom.GetComponent<EthanRoom>().rightRoom = gameObject;
        }

        Vector2Int rightLocation = location + new Vector2Int(ROOM_SIZE, 0);
        if (rooms.ContainsKey(rightLocation)) //if there is a room above
        {
            rightRoom = rooms[rightLocation];
            rightRoom.GetComponent<EthanRoom>().leftRoom = gameObject;
        }

        
    }
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        int randomIndex = UnityEngine.Random.Range(0, backgrounds.Length);
        spriteRenderer.sprite = backgrounds[randomIndex];
    }

    //Should be called when we want to enter the room
    void StartEnterRoom()
    {

    }

    //Should be called when we want to finish entering the room (unfreeze enemies
    void EndEnterRoom()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
