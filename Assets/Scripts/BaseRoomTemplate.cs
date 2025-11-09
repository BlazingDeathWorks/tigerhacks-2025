using System.Collections.Generic;
using UnityEngine;

public class BaseRoomTemplate : MonoBehaviour
{
    //This baseRoom gameobject contains a bunch of children that represent walls/spikes/enemies. 
    //When the base is started 
    List<GameObject> enemies = new List<GameObject>();
    bool _enabled = false;
    bool enemiesDefeated = false;

    void OnEnable()
    {
        _enabled = true;
        UnityEngine.Debug.Log("Enabling base room template");
        //On enable, explicitely enable all the enemies only if the level is not in a cleared state
        if (!transform.parent.GetComponent<EthanRoom>().roomCleared)
        {
            foreach (GameObject enemy in enemies)
            {
                enemy.SetActive(true);
            }
        } else
        {
            foreach (GameObject enemy in enemies)
            {
                enemy.SetActive(false);
            }
        }
    }

    void OnDisable()
    {
        _enabled = false;
        UnityEngine.Debug.Log("Disabling base room template");
        //probably not needed, but explicitely disable all the enemies
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(false);
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enabled = false;
        //Build an array of children elements that are enemies
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            if (childTransform.Find("EnemyCrossfire") != null || childTransform.Find("EnemyGnat") != null
                || childTransform.Find("EnemyLancer") != null)
            {
                enemies.Add(childTransform.gameObject);
            }
        }



            //TODO Fill the enemy types 
        }

    // Update is called once per frame
    void Update()
    {
        if (_enabled && !enemiesDefeated)
        {
            bool allDead = true;
            foreach (GameObject enemy in enemies)
            {
                if (enemy.activeSelf) allDead = false;
            }
            if (allDead)
            {
                enemiesDefeated = true;
                transform.parent.gameObject.GetComponent<EthanRoom>().ClearRoom();
            }
        }
        
    }
}
