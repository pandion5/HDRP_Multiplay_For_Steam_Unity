using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectGameManager : MonoBehaviour
{
    public static KinectGameManager instance;

    void Awake()
    {
        instance = this;
    }
    [Header("Charactor")]
    public AvatarController avatarController;

    [Header("Target")]
    public GameObject Target;

    [Header("DollyCart")]
    public GameObject player;
    public float moveSpeed = 3.4f;

    [Header("Kinect")]
    public KinectManager kinectManager;
    public GameObject feetCollider;
    public GameObject neck;

    [Header("Feets")]
    public GameObject leftFeet;
    public GameObject rightFeet;

    [Header("Colliders")]
    public GameObject leftFeetCollider;
    public GameObject rightFeetCollider;

    [Space]
    [Header("Current Steps")]
    public bool leftStep;
    public bool rightStep;

    private bool prevLeftStep;
    private bool prevRightStep;

    [Header("Walking Rate")]
    public int deltaStep;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(deltaStepInit());
    }
    
    public void StartWalking()
    {
        StartCoroutine(deltaStepInit());
    }

    IEnumerator deltaStepInit()
    {
        if (player)
            while (true)
            {
                deltaStep = 0;
                
                yield return new WaitForSeconds(3f);
                /*
                if (deltaStep < 1)
                    player.GetComponent<Rigidbody>().velocity = Vector3.forward;// * 0.1f;
                else if (deltaStep == 1)
                    player.GetComponent<Rigidbody>().velocity = Vector3.forward * moveSpeed;
                else
                    player.GetComponent<Rigidbody>().velocity = Vector3.forward * moveSpeed + Vector3.forward * (deltaStep - 2);
                */
                
            }
    }

    private void FixedUpdate()
    {
        if(player)
            if (neck)
            {
                //Debug.Log(neck.transform.localRotation);
                Target.transform.rotation = Quaternion.Euler(new Vector3(0, player.transform.eulerAngles.y+neck.transform.eulerAngles.y, 0));
            }
            else
            {
                Target.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
    }

    // Update is called once per frame
    void Update()
    {
        if(kinectManager.avatarControllers.Count == 1)
        {
            if (!avatarController)
            {
                if (leftFeet == null && rightFeet == null)
                {
                    avatarController = kinectManager.avatarControllers[0];
                    Setup(avatarController);
                    return;
                }
            }

            Walk();
        } 
    }

    private void LateUpdate()
    {
        if (kinectManager.avatarControllers.Count == 1)
        {
                if (deltaStep < 1)
                    player.GetComponent<Rigidbody>().velocity = Target.transform.forward * 0.1f / 2;
                else if (deltaStep == 1)
                    player.GetComponent<Rigidbody>().velocity = Target.transform.forward * moveSpeed / 2;
                else
                    player.GetComponent<Rigidbody>().velocity = (Target.transform.forward * moveSpeed + Vector3.forward * (deltaStep - 2)) / 2;
                Debug.Log("test");
        }
    }

    void Walk()
    {
        bool state = false;

        if (prevRightStep == rightStep && prevLeftStep == leftStep)
        {
            return;
        }

        if (prevRightStep == prevLeftStep)
        {
            prevLeftStep = leftStep;
            prevRightStep = rightStep;
            state = false;
        }
        else
        {
            prevLeftStep = leftStep;
            prevRightStep = rightStep;
            state = true;
        }

        if(state)
        {
            deltaStep++;
        }
    }

    void Setup(AvatarController avatarControllers)
    {
        if (leftFeet = GameObject.Find("mixamorig2:LeftToeBase"))
        {
            if (rightFeet = GameObject.Find("mixamorig2:RightToeBase"))
            {
                leftFeetCollider = GameObject.Instantiate(feetCollider);
                rightFeetCollider = GameObject.Instantiate(feetCollider);

                leftFeetCollider.transform.SetParent(leftFeet.transform);
                rightFeetCollider.transform.SetParent(rightFeet.transform);
            }
        }

        if (neck = GameObject.Find("mixamorig2:Neck"))
        {
            return;
        }
    }
}
