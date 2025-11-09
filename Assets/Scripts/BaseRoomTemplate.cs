using UnityEngine;

public class BaseRoomTemplate : MonoBehaviour
{
    //This baseRoom gameobject contains a bunch of children that represent walls/spikes/enemies. 
    //When the base is started 

    void OnEnable()
    {
        UnityEngine.Debug.Log("Enabling base room template");
        //On enable, explicitely enable all the enemies only if the level is not in a cleared state
        if (!transform.parent.GetComponent<EthanRoom>().roomCleared)
        {
            //TODO enable the enemies, but  (MAYBE) keep them frozen
        } else
        {
            //TODO dont enable the enemies
        }
    }

    void OnDisable()
    {
        UnityEngine.Debug.Log("Disabling base room template");
        //probably not needed, but explicitely disable all the enemies
    }

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
        //TODO Fill the enemy types 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
