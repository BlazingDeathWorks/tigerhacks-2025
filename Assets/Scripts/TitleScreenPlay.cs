using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenPlay : MonoBehaviour
{
    public Image fadeImage;

    bool clickedButton = false;
    bool finishedScreenFade = false;
    void OnMouseDown()
    {
        UnityEngine.Debug.Log("Button Down!");
        if (Input.GetMouseButtonDown(0) && !clickedButton)
        {
            StartCoroutine(FadeToBlack());
            clickedButton = true;
            UnityEngine.Debug.Log("Button pressed!");
        }
    }

    IEnumerator FadeToBlack()
    {
        fadeImage.gameObject.SetActive(true); // Ensure the image is active
        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / 1);
            Color newColor = fadeImage.color;
            newColor.a = alpha;
            fadeImage.color = newColor;
            yield return null; // Wait for the next frame
        }

        Color finalColor = fadeImage.color;
        finalColor.a = 1.0f;
        fadeImage.color = finalColor;
        finishedScreenFade = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (finishedScreenFade)
        {
            SceneManager.LoadScene("Level1");
        }
    }
}
