using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [SerializeField] private int doorDirection;

    private bool doorEnabled = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void EnableDoor()
    {
        this.doorEnabled = true;
    }
    public void DisableDoor()
    {
        this.doorEnabled = false;
    }

    void Start()
    {
                
    }

    public void Open()
    {
        if (this.doorEnabled)
        {
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public void Close()
    {
        if (this.doorEnabled)
        {
            this.GetComponent<SpriteRenderer>().enabled = true;
            this.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!this.doorEnabled) return;
        if (!transform.parent.gameObject.GetComponent<EthanRoom>().roomActive)
        {
            //Since the room is not active, this should active like a transition since the only way to hit it is if we wer enot in the room
            //we will signal up to the room controller that we want to do a room transition
            transform.parent.gameObject.GetComponent<EthanRoom>().previousActiveRoom = doorDirection;
            transform.parent.gameObject.GetComponent<EthanRoom>().triggerRoomChange = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
