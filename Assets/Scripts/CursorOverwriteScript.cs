using UnityEngine;

public class CursorOverwriteScript : MonoBehaviour
{

    public Texture2D cursorTexture; // Assign your default cursor image in the Inspector
    public Texture2D clickedCursorTexture;
    public Vector2 cursorHotspot = Vector2.zero; // Hotspot of the cursor (e.g., where the click registers)



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Set a different cursor for clicking
        {
            // Set a different cursor for clicking, if desired
            // Cursor.SetCursor(clickedCursorTexture, cursorHotspot, CursorMode.Auto);
        }
        if (Input.GetMouseButtonUp(0)) // Revert back to normal image
        {
            // Revert to the default cursor
            // Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }
}
