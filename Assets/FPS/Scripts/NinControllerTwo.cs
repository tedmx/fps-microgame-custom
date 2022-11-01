using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class NinControllerTwo : MonoBehaviour
{
    public bool leftFeetMovesBack = false;
    public bool rightFeetMovesBack = false;
    public float forwardMoveSpeed = 0;

    public float floorStepImpact = 0;

    public bool navigationIsStopped = false;

    Vector3 lastLeftLegIKPos;
    Vector3 lastRightLegIKPos;

    // Start is called before the first frame update
    void Start()
    {
    }

    void UpdatePlayerTrackingSight()
    {
        var neckBone = GameObject.Find("Neck");
        var player = GameObject.Find("Player");

        var quaternionToPlayer = new Quaternion();
        quaternionToPlayer.SetLookRotation((player.transform.position - neckBone.transform.position).normalized, Vector3.up);

        var targetRotation = quaternionToPlayer;

        var vectorToPlayerYAgnostic = player.transform.position - transform.position;
        vectorToPlayerYAgnostic.y = 0;


        var angleToPlayerYAgnostic = Vector3.Angle(vectorToPlayerYAgnostic, transform.forward);

        var playerIsBehind = angleToPlayerYAgnostic > 100;
        var playerIsTooFarAway = vectorToPlayerYAgnostic.magnitude > 17;

        if (playerIsBehind || playerIsTooFarAway)
        {
            targetRotation = new Quaternion();
            targetRotation.SetLookRotation(transform.forward);
        }

        neckBone.transform.rotation = Quaternion.Slerp(targetRotation, neckBone.transform.rotation, 0.999f);
    }

    void UpdateCameraShake()
    {
        var player = GameObject.Find("Player");
        var mainCamera = GameObject.Find("Main Camera");

        var shakeController = mainCamera.GetComponent("CameraShake") as CameraShake;
        var distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        var distanceDampenFactor = Mathf.Min(1, 1 / distanceToPlayer);
        shakeController.forceShakingSwitch = floorStepImpact > 0;
        shakeController.shakeAmount = floorStepImpact * distanceDampenFactor;
    }

    void UpdateWalk()
    {
        var curLeftLegIKPos = GameObject.Find("LegL_IK_Control").transform.position - transform.position;
        var curRightLegIKPos = GameObject.Find("LegR_IK_Control").transform.position - transform.position;

        var NavMeshAgent = GetComponent<NavMeshAgent>();

        var animator = GetComponent<Animator>();

        NavMeshAgent.speed = 0;

        Debug.Log("remainingDistance");
        Debug.Log(NavMeshAgent.remainingDistance);

        animator.SetBool("isWalking", NavMeshAgent.remainingDistance > 5);

        if (leftFeetMovesBack)
        {
            var increment = (lastLeftLegIKPos - curLeftLegIKPos).magnitude;
            // transform.position = transform.position + transform.forward * increment;
            var distance = (lastLeftLegIKPos - curLeftLegIKPos).magnitude;
            NavMeshAgent.speed = distance / Time.deltaTime;
        }
        else if (rightFeetMovesBack)
        {
            var increment = (lastRightLegIKPos - curRightLegIKPos).magnitude;
            // transform.position = transform.position + transform.forward * increment;
            //NavMeshAgent.speed = increment * 125;
            var distance = (lastRightLegIKPos - curRightLegIKPos).magnitude;
            NavMeshAgent.speed = distance / Time.deltaTime;
        }

        lastLeftLegIKPos = curLeftLegIKPos;
        lastRightLegIKPos = curRightLegIKPos;
    }
    void Navigate()
    {
        var NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent.isStopped = navigationIsStopped;
        var target = GameObject.Find("NinTarget");
        NavMeshAgent.SetDestination(target.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerTrackingSight();

        UpdateWalk();

        UpdateCameraShake();

        Navigate();

        var upperBodyBone = GameObject.Find("UpperBody");

        upperBodyBone.transform.rotation = transform.rotation;
        var rotationDegToSet = 45 * Mathf.Sin(Time.time / 5);
        upperBodyBone.transform.Rotate(0, rotationDegToSet, 0);

    }
}
