using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class MapGeneratorScript : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    private Dictionary<Vector2Int, GameObject> rooms = new Dictionary<Vector2Int, GameObject>();
    private int roomCount = 0;
    private int targetRoomCount;
    private int deadEndBranchLengthMin;
    private int deadEndBranchLengthMax;
    private int ROOM_SIZE = 16 * 4;
    private Vector2Int end;

    
    public void Generate(Vector2Int start, Vector2Int end, int targetRoomCount, int deadEndBranchLengthMin, int deadEndBranchLengthMax)
    {

        this.targetRoomCount = targetRoomCount;
        this.deadEndBranchLengthMax= deadEndBranchLengthMax;
        this.deadEndBranchLengthMin= deadEndBranchLengthMin;
        this.end = end;

        //Generate the main path and place the start and end
        GenerateMainPath(start, end);

        //Add branches and deadends
        AddBranches();

        //assign room presets
        //AssignRoomPresets(start, end);

        //add scaled monsters based on distance to boss room
        //each room will just have a list of monsters it will spawn when the map is loaded
        //the list will contain state information about the monsters, such as health, last location, etc

    }

    public void GenerateMainPath(Vector2Int start, Vector2Int end)
    {
        //Place the start and end rooms
        rooms[start] = Instantiate(roomPrefab, new Vector3(start.x, start.y, 0f), Quaternion.identity);
        rooms[end] = Instantiate(roomPrefab, new Vector3(end.x, end.y, 0f), Quaternion.identity);

        //TEMP CODE TO COLOR START AND END
        SpriteRenderer spriteRenderer = rooms[end].transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0f, 0f, 1f, 0.5f);
        SpriteRenderer spriteRenderer2 = rooms[start].transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer2.color = new Color(0f, 1f, 0f, 0.5f);


        //just based on the orientation of end relative to the current location, roll a weighted coin
        //based on how close we are in either direction 
        Vector2Int currentPosition = start;
        while (currentPosition != end)
        {
            Vector2Int nextPosition = currentPosition + GetWeightedDirection(currentPosition, end);
            if (nextPosition != end)
            {
                rooms[nextPosition] = Instantiate(roomPrefab, new Vector3(nextPosition.x, nextPosition.y, 0f), Quaternion.identity);
                EthanRoom roomScript = rooms[nextPosition].GetComponent<EthanRoom>();
                roomScript.Initialize(rooms, nextPosition, ROOM_SIZE);
                targetRoomCount++;
            }
            currentPosition = nextPosition;
            if (roomCount > 5) break;
        }
        
        //for each room we place, set its door accordingly, we may need to add more later
    }

    private void AddBranches()
    {
        //choose a random room that is not the end room
        //pick a random direction with a room and add it. 
        //continue doing this until we get to a randomly set depth or until we run out of expansions
        //then choose a new random room until we reach the maximum number of rooms

        while (targetRoomCount > roomCount)
        {
            //choose a random room in the rooms list
            Vector2Int location = GetRandomKey(rooms);
            if (location == end) continue;
            for (int i = 0; i < UnityEngine.Random.Range(this.deadEndBranchLengthMin, this.deadEndBranchLengthMax); i++)
            {
                Vector2Int next = PlaceRandomNeighboringRoom(location);
                if (location == next) break; //if the place failed because there is nowhere else to place
                location = next;
            }
        }
    }

    public Vector2Int GetRandomKey(Dictionary<Vector2Int, GameObject> dict)
    {

        // Get all keys into a List
        List<Vector2Int> keys = dict.Keys.ToList();

        // Generate a random index
        int randomIndex = UnityEngine.Random.Range(0, keys.Count); // Max is exclusive for integers

        // Return the key at the random index
        return keys[randomIndex];
    }

    private Vector2Int PlaceRandomNeighboringRoom(Vector2Int location)
    {
        //randomly sort a list of [1, 2, 3, 4]
        int[] unShuffledDirections = new int[4] { 0, 1, 2, 3 };
        int[] directions = unShuffledDirections.OrderBy(x => UnityEngine.Random.value).ToArray();
        UnityEngine.Debug.Log(directions[0]);

        for (int i = 0; i < 4; i++)
        {
            switch(directions[i])
            {
                //TODO the issue here is that we need to check wouldSurroundLocation on all 4 neighbors
                case 0:
                    Vector2Int topLocationS = location + new Vector2Int(0, ROOM_SIZE);
                    if (!rooms.ContainsKey(topLocationS)) //if there is a room above
                    {
                        if (!LocationIsSurrounded(topLocationS) && 
                            !WouldSurroundLocation(topLocationS + new Vector2Int(0, ROOM_SIZE), 1) &&
                            !WouldSurroundLocation(topLocationS + new Vector2Int(0, -ROOM_SIZE), 0) &&
                            !WouldSurroundLocation(topLocationS + new Vector2Int(ROOM_SIZE, 0), 2) &&
                            !WouldSurroundLocation(topLocationS + new Vector2Int(-ROOM_SIZE, 0), 3))
                        {
                            PlaceRoom(topLocationS); return topLocationS;
                        }
                    }
                    break;
                case 1:
                    Vector2Int bottomLocationS = location + new Vector2Int(0, -ROOM_SIZE);
                    if (!rooms.ContainsKey(bottomLocationS)) //if there is a room above
                    {
                        if (!LocationIsSurrounded(bottomLocationS) &&
                            !WouldSurroundLocation(bottomLocationS + new Vector2Int(0, ROOM_SIZE), 1) &&
                            !WouldSurroundLocation(bottomLocationS + new Vector2Int(0, -ROOM_SIZE), 0) &&
                            !WouldSurroundLocation(bottomLocationS + new Vector2Int(ROOM_SIZE, 0), 2) &&
                            !WouldSurroundLocation(bottomLocationS + new Vector2Int(-ROOM_SIZE, 0), 3))
                        {
                            PlaceRoom(bottomLocationS); return bottomLocationS;
                        }
                    }
                    break;
                case 2:
                    Vector2Int leftLocationS = location + new Vector2Int(-ROOM_SIZE, 0);
                    if (!rooms.ContainsKey(leftLocationS)) //if there is a room above
                    {
                        if (!LocationIsSurrounded(leftLocationS) &&
                            !WouldSurroundLocation(leftLocationS + new Vector2Int(0, ROOM_SIZE), 1) &&
                            !WouldSurroundLocation(leftLocationS + new Vector2Int(0, -ROOM_SIZE), 0) &&
                            !WouldSurroundLocation(leftLocationS + new Vector2Int(ROOM_SIZE, 0), 2) &&
                            !WouldSurroundLocation(leftLocationS + new Vector2Int(-ROOM_SIZE, 0), 3))
                        {
                            PlaceRoom(leftLocationS); return leftLocationS;
                        }
                    }
                    break;
                case 3:
                    Vector2Int rightLocationS = location + new Vector2Int(ROOM_SIZE, 0);
                    if (!rooms.ContainsKey(rightLocationS)) //if there is a room above
                    {
                        if (!LocationIsSurrounded(rightLocationS) &&
                            !WouldSurroundLocation(rightLocationS + new Vector2Int(0, ROOM_SIZE), 1) &&
                            !WouldSurroundLocation(rightLocationS + new Vector2Int(0, -ROOM_SIZE), 0) &&
                            !WouldSurroundLocation(rightLocationS + new Vector2Int(ROOM_SIZE, 0), 2) &&
                            !WouldSurroundLocation(rightLocationS + new Vector2Int(-ROOM_SIZE, 0), 3))
                        {
                            PlaceRoom(rightLocationS); return rightLocationS;
                        }
                    }
                    break;
            }
        }

        //if we got here, then all 4 locations were taken, so just return the current location to indicate no room was placed
        return location;
    }

    private void PlaceRoom(Vector2Int location)
    {
        rooms[location] = Instantiate(roomPrefab, new Vector3(location.x, location.y, 0f), Quaternion.identity);
        EthanRoom roomScript = rooms[location].GetComponent<EthanRoom>();
        roomScript.Initialize(rooms, location, ROOM_SIZE);
        roomCount++;
    }

    private bool LocationIsSurrounded(Vector2Int location)
    {
        if (rooms.ContainsKey(location + new Vector2Int(0, ROOM_SIZE)) &&
            rooms.ContainsKey(location + new Vector2Int(0, -ROOM_SIZE)) &&
            rooms.ContainsKey(location + new Vector2Int(ROOM_SIZE, 0)) &&
            rooms.ContainsKey(location + new Vector2Int(-ROOM_SIZE, 0))) return true;
        return false;
    }

    private bool WouldSurroundLocation(Vector2Int location, int newLocation)
    {
        if (!rooms.ContainsKey(location)) return false; 

        bool left = false;
        bool right = false;
        bool up = false;
        bool down = false;


        if (rooms.ContainsKey(location + new Vector2Int(0, ROOM_SIZE))) up = true;
        if (rooms.ContainsKey(location + new Vector2Int(0, -ROOM_SIZE))) down = true;
        if (rooms.ContainsKey(location + new Vector2Int(-ROOM_SIZE, 0))) left = true;
        if (rooms.ContainsKey(location + new Vector2Int(ROOM_SIZE, 0))) right = true;

        switch (newLocation) {
            case 0:
                up = true;
                break;
            case 1:
                down = true;
                break;
            case 2:
                left = true;
                break;
            case 3:
                right = true;
                break;
        }

        if (up && down && left && right) return true;

        return false;
    }



    private Vector2Int GetWeightedDirection(Vector2Int current, Vector2Int target)
    {
        int deltaX = target.x - current.x;
        int deltaY = target.y - current.y;

        // If we're on a straight path (only one direction needed)
        if (deltaX == 0)
        {
            return new Vector2Int(0, deltaY > 0 ? ROOM_SIZE : -ROOM_SIZE);
        }
        if (deltaY == 0)
        {
            return new Vector2Int(deltaX > 0 ? ROOM_SIZE : -ROOM_SIZE, 0);
        }

        // Both directions bring us closer - do weighted coin flip
        int absX = Mathf.Abs(deltaX);
        int absY = Mathf.Abs(deltaY);
        int totalDistance = absX + absY;

        // Probability of choosing X direction is proportional to X distance
        float xProbability = (float)absX / totalDistance;

        if (UnityEngine.Random.value < xProbability)
        {
            // Move in X direction
            return new Vector2Int(deltaX > 0 ? ROOM_SIZE : -ROOM_SIZE, 0);
        }
        else
        {
            // Move in Y direction
            return new Vector2Int(0, deltaY > 0 ? ROOM_SIZE : -ROOM_SIZE);
        }
    }


    public void AssignRoomPresets(Vector2Int start, Vector2Int end)
    {
        //just iterate over each room and assign a preset. If its specifically the start and end room, assign different presets

    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate(Vector2Int.zero, new Vector2Int(ROOM_SIZE * 5, ROOM_SIZE * 6), 10, 4, 7);
        /*
        GameObject roomtest = Instantiate(roomPrefab, Vector2.zero, Quaternion.identity);
        EthanRoom roomScript = roomtest.GetComponent<EthanRoom>();
        roomScript.Initialize(true, true, true, true);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
