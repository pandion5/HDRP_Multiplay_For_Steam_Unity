using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    public bool isKinect = false;
    private GameObject spawnedPlayerPrefab;
    private GameObject Player;
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Player = GameObject.Find("Player");
        if (GameObject.Find("KinectGameManager"))
        {
            isKinect = true;
        }
        else
        {
            Player.transform.position = transform.position;
        }
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        if(isKinect)
        {
            GameObject lookAt = GameObject.Find("LookAt");
            lookAt.transform.SetParent(spawnedPlayerPrefab.transform);
            lookAt.transform.SetPositionAndRotation(this.transform.position + new Vector3(0,2,0), Quaternion.Euler(new Vector3(0,0,0)));
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}
