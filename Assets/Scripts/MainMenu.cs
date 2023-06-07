using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    //local multiplayer button
    public Button localButton;

    //online multiplayer button
    public Button onlineButton;

    //exit game button
    public Button quitButton;

    //the sound controller
    private SoundController soundController;

    // Start is called before the first frame update
    void Start()
    {
        //get the sound controller
        soundController = GameObject.FindGameObjectWithTag("SFX").GetComponent<SoundController>();

        //configure buttons
        localButton.onClick.AddListener(() =>
        {
            soundController.playClick();
            SceneManager.LoadScene("Single");
        });
        onlineButton.onClick.AddListener(() =>
        {
            soundController.playClick();
            SceneManager.LoadScene("Multi");
        });
        quitButton.onClick.AddListener(() =>
        {
            soundController.playClick();
            Application.Quit();
        });
    }
}
