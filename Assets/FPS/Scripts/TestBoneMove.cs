using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class TestBoneMove : MonoBehaviour
{
    public bool leftFeetMovesBack = false;
    public bool rightFeetMovesBack = false;
    public float forwardMoveSpeed = 0;

    Vector3 lastLeftLegIKPos;
    Vector3 lastRightLegIKPos;

    // Start is called before the first frame update
    void Start()
    {
        // var anim = gameObject.GetComponent<Animation>();
        // anim.Play("walk");
    }

    // Update is called once per frame
    void Update()
    {
        var bone = GameObject.Find("UpperBody");
        var currentAngles = bone.transform.localEulerAngles;
        // Debug.Log(currentAngles);
        bone.transform.rotation = Quaternion.identity;
        var rotationDegToSet = -90 + (-45) * Mathf.Sin(Time.time / 5);
        bone.transform.Rotate(0, rotationDegToSet, 0);

        var neckBone = GameObject.Find("Neck");
        var player = GameObject.Find("Player");
        var direction = (player.transform.position - neckBone.transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(direction);

        neckBone.transform.rotation = Quaternion.Slerp(lookRotation, neckBone.transform.rotation, 0.999f);

        var curLeftLegIKPos = GameObject.Find("LegL_IK_Control").transform.position;
        var curRightLegIKPos = GameObject.Find("LegR_IK_Control").transform.position;

        if (leftFeetMovesBack) {
            var increment = (lastLeftLegIKPos.x - curLeftLegIKPos.x);
            Debug.Log(curLeftLegIKPos.x - lastLeftLegIKPos.x);
            transform.position = transform.position + new Vector3(increment, 0, 0);
        } else if (rightFeetMovesBack) {
            var increment = (lastRightLegIKPos.x - curRightLegIKPos.x);
            Debug.Log(curLeftLegIKPos.x - lastLeftLegIKPos.x);
            transform.position = transform.position + new Vector3(increment, 0, 0);
        }


        lastLeftLegIKPos = curLeftLegIKPos;
        lastRightLegIKPos = curRightLegIKPos;
        /*if (forwardMoveSpeed > 0) { 
            transform.position = transform.position + new Vector3(-1 * forwardMoveSpeed * Time.deltaTime, 0, 0);
        }*/

        // var rightLegIK = GameObject.Find("LegR_IK_Control");

        // rightLegIK.transform.position = rightLegIK.transform.position + new Vector3(-0.006f, 0.004f, 0);

        // Debug.Log(GameObject.Find("UpperBody"));
    }
}
