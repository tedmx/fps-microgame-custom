using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class NinController : MonoBehaviour
{
    public bool leftFeetMovesBack = false;
    public bool rightFeetMovesBack = false;
    public float forwardMoveSpeed = 0;

    public float floorStepImpact = 0;

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

        bone.transform.rotation = Quaternion.identity;
        var rotationDegToSet = -90 + (-45) * Mathf.Sin(Time.time / 5);
        bone.transform.Rotate(0, rotationDegToSet, 0);


        var neckBone = GameObject.Find("Neck");
        var player = GameObject.Find("Player");
        var direction = (player.transform.position - neckBone.transform.position).normalized;

        var lookAtPlayerQuaternion = Quaternion.LookRotation(direction);

        var playerPositionYAgnostic = player.transform.position;
        playerPositionYAgnostic.y = 0;

        var thisPositionYAgnostic = transform.position;
        playerPositionYAgnostic.y = 0;

        var vectorToPlayer = playerPositionYAgnostic - thisPositionYAgnostic;

        // var vectorToPlayer = playerPositionYAgnostic - thisPositionYAgnostic;

        // var targetVector = lookAtPlayerQuaternion;
        var targetQuaternion = Quaternion.Euler(vectorToPlayer.x, vectorToPlayer.y, vectorToPlayer.z);
        // var forwardVector = transform.forward;
        // var adjustedForwardVector = new Vector3(-90, 0, 0);
        //Debug.Log("adjustedForwardVector");
        //Debug.Log(adjustedForwardVector);

        var forwardQuaternion = Quaternion.Euler(0, -90, 0);

        var angleToPlayerYAgnostic = Vector3.Angle(vectorToPlayer, forwardQuaternion.eulerAngles);
        var playerIsBehind = angleToPlayerYAgnostic > 100;

        var playerIsTooFarAway = vectorToPlayer.magnitude > 17;

        if (playerIsBehind || playerIsTooFarAway)
        {
            targetQuaternion = forwardQuaternion;
        }

        var newRotation = neckBone.transform.rotation;
        newRotation.SetFromToRotation(neckBone.transform.forward, vectorToPlayer);
        neckBone.transform.rotation = newRotation;// Quaternion.Euler(vectorToPlayer.x, vectorToPlayer.y, vectorToPlayer.z);
        // neckBone.transform.rotation = Quaternion.Slerp(targetQuaternion, neckBone.transform.rotation, 0.999f);

        /* var correctedNeckRotationY = neckBone.transform.localEulerAngles.y;
         var limitDistances = new float[] { 0, 0 };
         if (correctedNeckRotationY > 45 && correctedNeckRotationY < 315)
         {
             limitDistances[0] = Math.Abs(correctedNeckRotationY - 45);
             limitDistances[1] = Math.Abs(correctedNeckRotationY - 315);
             if (limitDistances[0] < limitDistances[1])
             {
                 correctedNeckRotationY = 45;
             } else
             {
                 correctedNeckRotationY = 315;
             }
         }*/
        //Debug.Log("correctedNeckRotationY");
        //Debug.Log(correctedNeckRotationY);


        // var neckLocalEulerAngles = neckBone.transform.localEulerAngles;
        //neckLocalEulerAngles.y = correctedNeckRotationY;

        // neckBone.transform.localEulerAngles = neckLocalEulerAngles;

        var curLeftLegIKPos = GameObject.Find("LegL_IK_Control").transform.position - transform.position;
        var curRightLegIKPos = GameObject.Find("LegR_IK_Control").transform.position - transform.position;

        if (leftFeetMovesBack)
        {
            var increment = (lastLeftLegIKPos.x - curLeftLegIKPos.x);
            transform.position = transform.position + new Vector3(increment, 0, 0);
        } else if (rightFeetMovesBack)
        {
            var increment = (lastRightLegIKPos.x - curRightLegIKPos.x);
            transform.position = transform.position + new Vector3(increment, 0, 0);
        }

        lastLeftLegIKPos = curLeftLegIKPos;
        lastRightLegIKPos = curRightLegIKPos;

        var mainCamera = GameObject.Find("Main Camera");

        var shakeController = mainCamera.GetComponent("CameraShake") as CameraShake;
        var distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        var distanceDampenFactor = Mathf.Min(1, 1 / distanceToPlayer);
        shakeController.forceShakingSwitch = floorStepImpact > 0;
        shakeController.shakeAmount = floorStepImpact * distanceDampenFactor;

    }
}
