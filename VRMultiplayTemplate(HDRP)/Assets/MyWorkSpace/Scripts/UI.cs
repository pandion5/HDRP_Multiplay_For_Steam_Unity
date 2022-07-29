using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public GameObject connect_UI;
    public GameObject avatarSelection_UI;
    public GameObject roomSet_UI;

    public void ConnectButton()
    {
        connect_UI.SetActive(false);
        avatarSelection_UI.SetActive(true);
    }

    public void SelectAvatar1()
    {

    }

    public void SelectAvatar2()
    {

    }

    public void SelectAvatar3()
    {

    }
    
    public void SelectButton()
    {
        avatarSelection_UI.SetActive(false);
        roomSet_UI.SetActive(true);
    }

    public void Room1Button()
    {
        SceneManager.LoadScene("");
    }
}
