using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine;

public class EthanRoom : MonoBehaviour
{
    [SerializeField] private Sprite[] backgrounds;
    public GameObject player; 

    public GameObject leftRoom;
    public GameObject rightRoom;
    public GameObject topRoom;
    public GameObject bottomRoom;

    public GameObject roomTemplate;

    public GameObject itemChoiceManager;

    public GameObject mapGenerator;

    public GameObject boss;

    public string nextSceneName;

    public bool isBossRoom = false;
    public bool isStartRoom = false;
    public bool roomCleared = false;
    public bool roomActive = false;
    public int previousActiveRoom = -1;
    public bool triggerRoomChange = false;

    bool queueSceneChange = false;
    float timeSinceMapChangeStart = 0;
    
    public void Initialize(Dictionary<Vector2Int, GameObject> rooms, Vector2Int location, int ROOM_SIZE, GameObject player, GameObject roomTemplate, GameObject mapGenerator)
    {
        this.roomTemplate = roomTemplate;
        this.mapGenerator = mapGenerator;
        //Make sure all corners are properly set
        Vector2Int topLocation = location + new Vector2Int(0, ROOM_SIZE);
        if (rooms.ContainsKey(topLocation)) //if there is a room above
        {
            topRoom = rooms[topLocation];
            EnableTopDoor();
            topRoom.GetComponent<EthanRoom>().bottomRoom = gameObject;
            topRoom.GetComponent<EthanRoom>().EnableBottomDoor();
        }

        Vector2Int bottomLocation = location + new Vector2Int(0, -ROOM_SIZE);
        if (rooms.ContainsKey(bottomLocation)) //if there is a room above
        {
            bottomRoom = rooms[bottomLocation];
            EnableBottomDoor();
            bottomRoom.GetComponent<EthanRoom>().topRoom = gameObject;
            bottomRoom.GetComponent<EthanRoom>().EnableTopDoor();
        }

        Vector2Int leftLocation = location + new Vector2Int(-ROOM_SIZE, 0);
        if (rooms.ContainsKey(leftLocation)) //if there is a room above
        {
            leftRoom = rooms[leftLocation];
            EnableLeftDoor();
            leftRoom.GetComponent<EthanRoom>().rightRoom = gameObject;
            leftRoom.GetComponent<EthanRoom>().EnableRightDoor();
        }

        Vector2Int rightLocation = location + new Vector2Int(ROOM_SIZE, 0);
        if (rooms.ContainsKey(rightLocation)) //if there is a room above
        {
            rightRoom = rooms[rightLocation];
            EnableRightDoor();
            rightRoom.GetComponent<EthanRoom>().leftRoom = gameObject;
            rightRoom.GetComponent<EthanRoom>().EnableLeftDoor();
        }

        this.player = player;
        
    }

    public void EnableTopDoor()
    {
        transform.Find("TopDoor").GetComponent<DoorLogic>().EnableDoor();
    }

    public void EnableRightDoor()
    {
        transform.Find("RightDoor").GetComponent<DoorLogic>().EnableDoor();
    }

    public void EnableLeftDoor()
    {
        transform.Find("LeftDoor").GetComponent<DoorLogic>().EnableDoor();
    }

    public void EnableBottomDoor()
    {
        transform.Find("BottomDoor").GetComponent<DoorLogic>().EnableDoor();
    }

    void toggleLeftDoor(bool toggle)
    {
        GameObject door = this.transform.Find("LeftDoor").gameObject;
        if (leftRoom != null)
        {
            if (toggle)
            {
                door.GetComponent<DoorLogic>().Close();
            } else
            {
                door.GetComponent<DoorLogic>().Open();
            }
            
        }
    }

    void toggleRightDoor(bool toggle)
    {
        GameObject door = this.transform.Find("RightDoor").gameObject;
        if (rightRoom != null)
        {
            if (toggle)
            {
                door.GetComponent<DoorLogic>().Close();
            }
            else
            {
                door.GetComponent<DoorLogic>().Open();
            }
        }
    }

    void toggleTopDoor(bool toggle)
    {
        GameObject door = this.transform.Find("TopDoor").gameObject;
        if (topRoom != null)
        {
            if (toggle)
            {
                door.GetComponent<DoorLogic>().Close();
            }
            else
            {
                door.GetComponent<DoorLogic>().Open();
            }
        }
    }

    void toggleBottomDoor(bool toggle)
    {
        GameObject door = this.transform.Find("BottomDoor").gameObject;
        if (bottomRoom != null)
        {
            if (toggle)
            {
                door.GetComponent<DoorLogic>().Close();
            }
            else
            {
                door.GetComponent<DoorLogic>().Open();
            }
        }
    }

    public void DeactivateRoom()
    {
        toggleTopDoor(true);
        toggleBottomDoor(true);
        toggleRightDoor(true);
        toggleLeftDoor(true);
        this.roomActive = false;

        if (this.roomTemplate != null)
        {
            this.roomTemplate.SetActive(false);
        }
        else
        {
            UnityEngine.Debug.Log("Did not find base room template");
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
    public void StartEnterRoom(int enteringDirection) //0 == top, 1 == bottom, 2 == left, 3 == right
    {
        //Transform childTransform = transform.Find("BaseRoomTemplate");
        
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
    public void EndEnterRoom(int enteringDirection)
    {

        if (this.roomTemplate != null)
        {
            this.roomTemplate.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.Log("Did not find base room template");
        }

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

        }
    }



    public void ClearRoom()
    {
        this.roomCleared = true;
        if (!this.isBossRoom)
        {
            toggleLeftDoor(false);
            toggleRightDoor(false);
            toggleTopDoor(false);
            toggleBottomDoor(false);
            this.player.GetComponent<PlayerStatsManager>().Heal(30);
            if (!this.isStartRoom)
            {
                this.itemChoiceManager.SetActive(true);
                this.itemChoiceManager.GetComponent<ItemChoiceManager>().SpawnItemChoice();
            }
            
        } else
        {

            //update the progress params
            ProgressScript.Instance.roomCount += 6;
            ProgressScript.Instance.branchMaxCount += 1;
            ProgressScript.Instance.endDistance += 2;
            ProgressScript.Instance.HealthMultiplier += .5f;

            //TODO add some sort of animation here. maybe we wait for a boss death animation and then fade out the screen
            //then we transition to the next level
            mapGenerator.GetComponent<MapGeneratorScript>().FadeOut();

            queueSceneChange = true;
            //SceneManager.LoadScene(nextSceneName);
        }

    }

    // Update is called once per frame
    void Update()
    {

        //TODO TEMP - clear level on i
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!this.roomCleared && this.roomActive)
            {
                ClearRoom();
            }
        }

        if (queueSceneChange)
        {
            timeSinceMapChangeStart += Time.deltaTime;
        }

        if (timeSinceMapChangeStart > 2)
        {
            SceneManager.LoadScene(nextSceneName);
        }

        //Check if all the enemies are daed


        if (triggerRoomChange && previousActiveRoom != -1) //we want to transition to this room,
        {
            //UnityEngine.Debug.Log(Camera.main);
            switch (Camera.main.GetComponent<EthanCustomCamera>().transitionStatus)
            {
                case 0: //not yet started transition
                    //first, lock player movement
                    player.GetComponent<PlayerController>().LockMovement(); 

                    //now, call the startEnterRoom function and indicate we are actively in a room change
                    StartEnterRoom(previousActiveRoom);

                    //now wait until the camera transition
                    UnityEngine.Debug.Log("Running camera transition");
                    Camera.main.GetComponent<EthanCustomCamera>().runCameraTransition(previousActiveRoom);
                    

                    switch (previousActiveRoom)
                    {
                        case 0: //top
                            player.GetComponent<PlayerController>().TeleportToRelativePositionLocation(0, -1);
                            break;
                        case 1: //bottom
                            player.GetComponent<PlayerController>().TeleportToRelativePositionLocation(0, 1);
                            break;
                        case 2: //left
                            player.GetComponent<PlayerController>().TeleportToRelativePositionLocation(1, 0);
                            break;
                        case 3: //right
                            player.GetComponent<PlayerController>().TeleportToRelativePositionLocation(-1, 0);
                            break;
                    }

                    break;

                case 1: //actively transitioning, dont do anything
                    break;
                case 2: //transition finished
                    Camera.main.GetComponent<EthanCustomCamera>().transitionStatus = 0;

                    EndEnterRoom(previousActiveRoom);

                    triggerRoomChange = false;


                    //tell the previous room to become unactive
                    
                    //player.GetComponent<PlayerController>().TeleportToLocation(transform.position);

                    //unlock player controls
                    player.GetComponent<PlayerController>().UnlockMovement();

                    //update the active room
                    this.roomActive = true;

                    //tell the previous room to become unactive
                    switch (previousActiveRoom)
                    {
                        case 0: //top
                            topRoom.GetComponent<EthanRoom>().DeactivateRoom();
                            break;
                        case 1: //bottom
                            bottomRoom.GetComponent<EthanRoom>().DeactivateRoom();
                            break;
                        case 2: //left
                            leftRoom.GetComponent<EthanRoom>().DeactivateRoom();
                            break;
                        case 3: //right
                            rightRoom.GetComponent<EthanRoom>().DeactivateRoom();
                            break;
                    }

                    previousActiveRoom = -1;

                    break;
            }

        }
    }
}
