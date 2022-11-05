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

    bool nextCornerIsDefined;
    Vector3 nextCorner;

    Vector3 previousPosition = new();

    Vector3 lastLeftLegIKPos;
    Vector3 lastRightLegIKPos;

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

        var directionVector = transform.forward;
        if (nextCornerIsDefined)
        {
            directionVector = (nextCorner - transform.position).normalized;
        }

        if (leftFeetMovesBack)
        {
            var distance = (lastLeftLegIKPos - curLeftLegIKPos).magnitude;
            transform.position += directionVector * distance;

            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(transform.position, out navMeshHit, 10.0f, -1);
            if (navMeshHit.hit)
            {
                transform.position = navMeshHit.position;
            }
        }
        else if (rightFeetMovesBack)
        {
            var distance = (lastRightLegIKPos - curRightLegIKPos).magnitude;
            transform.position += directionVector * distance;

            NavMeshHit navMeshHit;
            NavMesh.SamplePosition(transform.position, out navMeshHit, 10.0f, -1);
            if (navMeshHit.hit)
            {
                transform.position = navMeshHit.position;
            }
        }

        lastLeftLegIKPos = curLeftLegIKPos;
        lastRightLegIKPos = curRightLegIKPos;

        previousPosition = transform.position;
    }
    void NavigateViaAgent()
    {
        var navMeshAgent = GetComponent<NavMeshAgent>();
        var target = GameObject.Find("NinTarget");

        // Debug.Log(NavMesh.Raycast(transform.position, target.transform.position, out hit, -1));

        Debug.Log("SamplePosition");
        Debug.Log(NavMesh.SamplePosition(target.transform.position, out NavMeshHit hit, 10.0f, -1));
        Debug.Log(hit);
        return;

        navMeshAgent.destination = GameObject.Find("NinTarget").transform.position;
        navMeshAgent.speed = 3;
    }

    void Navigate()
    {
        var animator = GetComponent<Animator>();
        var target = GameObject.Find("NinTarget");
        nextCornerIsDefined = false;

        /*Debug.Log("angleToCurPosition");
        Debug.Log(angleToCurPosition);*/

        NavMeshPath testPath = new();
        NavMesh.CalculatePath(transform.position, target.transform.position, 1, testPath);

        var distanceToTarget = (target.transform.position - transform.position).magnitude;
        if (testPath.corners.Length < 2 || distanceToTarget < 5)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        var vectorToNextPoint = (testPath.corners[1] - transform.position);

        var angleToNextPoint = Vector3.Angle(transform.forward, vectorToNextPoint);

        if (angleToNextPoint < 45)
        {
            animator.SetBool("isWalking", true);
        } else
        {
            animator.SetBool("isWalking", false);
        }

        Quaternion initialRotQuaternion = transform.rotation;

        transform.LookAt(testPath.corners[1]);
        Quaternion rotQuaternionToNextCorner = transform.rotation;

        nextCorner = testPath.corners[1];
        nextCornerIsDefined = true;

        transform.rotation = Quaternion.Slerp(rotQuaternionToNextCorner, initialRotQuaternion, 0.995f);
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
