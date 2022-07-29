using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public Transform root;

    public float turnSmoothness;

    public bool isKyle;

    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    //public Transform headConstraint;
    Vector3 headBodyOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (!root)
            root = transform;

        headBodyOffset = transform.position - head.rigTarget.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        root.position = head.rigTarget.position + headBodyOffset;
        if(isKyle)
            root.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(head.rigTarget.up, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
        else
            root.forward = Vector3.Lerp(transform.forward,Vector3.ProjectOnPlane(head.rigTarget.forward,Vector3.up).normalized, Time.deltaTime * turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
