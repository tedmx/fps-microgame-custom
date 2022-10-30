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

        var quaternionToPlayer = new Quaternion();
        quaternionToPlayer.SetLookRotation((player.transform.position - neckBone.transform.position).normalized, Vector3.up);

        var targetRotation = quaternionToPlayer;

        var vectorToPlayerYAgnostic = player.transform.position - transform.position;
        vectorToPlayerYAgnostic.y = 0;
        var angleToPlayerYAgnostic = Vector3.Angle(vectorToPlayerYAgnostic, new Vector3(-1, 0, 0));

        var playerIsBehind = angleToPlayerYAgnostic > 100;
        var playerIsTooFarAway = vectorToPlayerYAgnostic.magnitude > 17;

        if (playerIsBehind || playerIsTooFarAway)
        {
            targetRotation = Quaternion.Euler(0, -90, 0);
        }

        neckBone.transform.rotation = Quaternion.Slerp(targetRotation, neckBone.transform.rotation, 0.999f);

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
