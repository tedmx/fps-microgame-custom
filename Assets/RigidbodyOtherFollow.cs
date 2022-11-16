using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyOtherFollow : MonoBehaviour
{
    public GameObject target;
    public Vector3 positionOffset;
    BoneStatusInformer boneStatusInformer;
    Rigidbody thisRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        boneStatusInformer = target.GetComponent<BoneStatusInformer>();
        thisRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (boneStatusInformer.teleportRigidboneTrackers)
        {
            thisRigidbody.position = target.transform.position + positionOffset;
            thisRigidbody.rotation = target.transform.rotation;
            return;
        }
        // Debug.Log("RigidbodyOtherFollow::target.transform.position");
        // Debug.Log(target.transform.position);
        thisRigidbody.MovePosition(target.transform.position + positionOffset);
        thisRigidbody.MoveRotation(target.transform.rotation);
    }

    private void FixedUpdate()
    {
    }
}
