using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Unity.XR.CoreUtils;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class NetworkPlayer : MonoBehaviour
{
    public NetworkPlayerSpawner networkPlayerSpawner;
    public List<GameObject> avatars;
    public List<GameObject> kinectAvatars;

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    private PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;
    private GameObject spawnedAvatar;
    //--
    public SteamVR_Action_Vector2 input;
    public float speed = 1;

    private CharacterController characterController;

    // Start is called before the first frame update
    void Start()
    {
        networkPlayerSpawner = GameObject.Find("Network Manager").GetComponent<NetworkPlayerSpawner>();
        photonView = GetComponent<PhotonView>();
        if (!networkPlayerSpawner.isKinect)
        {
            XROrigin rig = FindObjectOfType<XROrigin>();
            headRig = rig.transform.Find("SteamVRObjects/VRCamera");
            leftHandRig = rig.transform.Find("SteamVRObjects/LeftHand");
            rightHandRig = rig.transform.Find("SteamVRObjects/RightHand");
        }
        else
        {
            GameObject.Find("KinectGameManager").GetComponent<KinectGameManager>().player = this.gameObject;
            GameObject.Find("KinectGameManager").GetComponent<KinectGameManager>().StartWalking();
            this.gameObject.GetComponent<Rigidbody>().freezeRotation = true;
        }
        if(photonView.IsMine)
            photonView.RPC("LoadAvatar", RpcTarget.AllBuffered, PlayerPrefs.GetInt("AvatarID"));

        
        characterController = GetComponent<CharacterController>();
    }

    //Function that is responsible to load an avatar among the avatar list
    [PunRPC]
    public void LoadAvatar(int index)
    {
        if (spawnedAvatar)
            Destroy(spawnedAvatar);

        if (!networkPlayerSpawner.isKinect)
        {
            spawnedAvatar = Instantiate(avatars[index], transform);
            AvatarInfo avatarInfo = spawnedAvatar.GetComponent<AvatarInfo>();
            avatarInfo.head.SetParent(head, false);
            avatarInfo.leftHand.SetParent(leftHand, false);
            avatarInfo.rightHand.SetParent(rightHand, false);

            leftHandAnimator = avatarInfo.leftHandAnimator;
            rightHandAnimator = avatarInfo.rightHandAnimator;
        }
        else
        {
            spawnedAvatar = Instantiate(kinectAvatars[index], transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            if (!networkPlayerSpawner.isKinect)
            {
                MapPosition(head, headRig);
                MapPosition(leftHand, leftHandRig);
                MapPosition(rightHand, rightHandRig);

                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
            }
        }
      
    }

    void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
    {
        if (!handAnimator)
            return;

        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }


    void MapPosition(Transform target,Transform rigTransform)
    {    
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }
}
