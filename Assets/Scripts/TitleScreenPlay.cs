using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenPlay : MonoBehaviour
{

    void OnMouseDown()
    {
        UnityEngine.Debug.Log("Button Down!");
        if (Input.GetMouseButtonDown(0))
        {
            UnityEngine.Debug.Log("LM down!");
            SceneManager.LoadScene("Level1");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
