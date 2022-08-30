using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetCounter : MonoBehaviour
{
    public string side;

    private void Start()
    {
        if (transform.parent.gameObject.name == "joint_ToeLT")
            side = "Left";
        else
            side = "Right";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("floor"))
            if (side == "Left")
                KinectGameManager.instance.leftStep = true;
            else
                KinectGameManager.instance.rightStep = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("floor"))
            if (side == "Left")
                KinectGameManager.instance.leftStep = false;
            else
                KinectGameManager.instance.rightStep = false;
    }
}
