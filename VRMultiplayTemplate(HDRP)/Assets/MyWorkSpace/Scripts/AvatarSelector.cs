using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    public void SetAvatarID(int index)
    {
        PlayerPrefs.SetInt("AvatarID", index);
    }
}
