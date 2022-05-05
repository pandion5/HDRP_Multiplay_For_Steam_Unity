using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    public NetworkManager networkManager;
    public AvatarSelector avatarSelector;
    // Start is called before the first frame update
    void Start()
    {
        networkManager.ConnectToServer();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))

        {
            avatarSelector.SetAvatarID(0);
            networkManager.InitiliazeRoom(0);
        }
    }
}
