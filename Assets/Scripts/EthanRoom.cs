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

    bool roomCleared = false;
    
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

    void toggleLeftDoor(bool toggle)
    {
        GameObject door = this.transform.Find("LeftDoor").gameObject;
        if (leftRoom != null)
        {
            door.GetComponent<SpriteRenderer>().enabled = toggle;
        }
    }

    void toggleRightDoor(bool toggle)
    {
        GameObject door = this.transform.Find("RightDoor").gameObject;
        if (rightRoom != null)
        {
            door.GetComponent<SpriteRenderer>().enabled = toggle;
        }
    }

    void toggleTopDoor(bool toggle)
    {
        GameObject door = this.transform.Find("TopDoor").gameObject;
        if (topRoom != null)
        {
            door.GetComponent<SpriteRenderer>().enabled = toggle;
        }
    }

    void toggleBottomDoor(bool toggle)
    {
        GameObject door = this.transform.Find("BottomDoor").gameObject;
        if (bottomRoom != null)
        {
            door.GetComponent<SpriteRenderer>().enabled = toggle;
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        int randomIndex = UnityEngine.Random.Range(0, backgrounds.Length);
        spriteRenderer.sprite = backgrounds[randomIndex];

        this.transform.Find("LeftDoor").GetComponent<SpriteRenderer>().enabled = true;
        this.transform.Find("RightDoor").GetComponent<SpriteRenderer>().enabled = true;
        this.transform.Find("TopDoor").GetComponent<SpriteRenderer>().enabled = true;
        this.transform.Find("BottomDoor").GetComponent<SpriteRenderer>().enabled = true;
    }

    //Should be called when we want to enter the room
    void StartEnterRoom(int enteringDirection) //0 == top, 1 == bottom, 2 == left, 3 == right
    {
        if (this.roomCleared)
        {
            this.toggleLeftDoor(false); this.toggleRightDoor(false);
            this.toggleTopDoor(false); this.toggleBottomDoor(false);
            //TODO make sure enemies dont get regenerated
        } else
        {
            switch (enteringDirection)
            {
                case 0:
                    this.toggleTopDoor(false);
                    break;
                case 1:
                    this.toggleBottomDoor(false);
                    break;
                case 2:
                    this.toggleLeftDoor(false);
                    break;
                case 3:
                    this.toggleRightDoor(false);
                    break;
            }

            //TODO nothing should be done here technically
        }
    }

    //Should be called after we finish the room entering animation
    void EndEnterRoom(int enteringDirection)
    {

        if (!this.roomCleared)
        {
            switch (enteringDirection)
            {
                case 0:
                    this.toggleTopDoor(true);
                    break;
                case 1:
                    this.toggleBottomDoor(true);
                    break;
                case 2:
                    this.toggleLeftDoor(true);
                    break;
                case 3:
                    this.toggleRightDoor(true);
                    break;
            }

            //TODO unfreeze the enemies
        }
    }

    void ClearRoom()
    {
        this.roomCleared = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
