using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.FPS.AI;
using Unity.FPS.Gameplay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class NinController : MonoBehaviour
{
    public bool leftFeetMovesBack = false;
    public bool rightFeetMovesBack = false;
    public float forwardMoveSpeed = 0;

    public float floorStepImpact = 0;

    public bool navigationIsStopped = false;

    bool nextCornerIsDefined;

    long lastHitTime;

    Vector3 nextCorner;

    Vector3 previousPosition = new();

    Vector3 lastLeftLegIKPos;
    Vector3 lastRightLegIKPos;

    Quaternion prevNeckRotation;

    Animator animator;
    public GameObject animatorHostGO;

    GameObject navTarget;

    bool doingSharpTurn = false;
    bool isDoingFightMove = false;

    public GameObject leftAnkleBone;

    public GameObject leftAnkleFastCollider;
    bool teleportLeftAnkleRigidbody = false;

    public GameObject rightWristBone;

    public GameObject rightWristFastCollider;
    bool teleportRightWristRigidbody = false;

    void Start()
    {
        animator = animatorHostGO.GetComponent<Animator>();
        prevNeckRotation = GameObject.Find("Neck").transform.rotation;
    }

    void UpdatePlayerTrackingSight()
    {
        var neckBone = GameObject.Find("Neck");

        var isKicking = animator.GetBool("shouldDoDownPunch");
        if (isKicking)
        {
            prevNeckRotation = neckBone.transform.rotation;
            return;
        }

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
            var isWalking = animator.GetBool("isWalking");
            if (isWalking)
            {
                targetRotation = neckBone.transform.rotation;
            } else
            {
                targetRotation = new Quaternion();
                targetRotation.SetLookRotation(transform.forward);
            }
        }

        neckBone.transform.rotation = Quaternion.Slerp(targetRotation, prevNeckRotation, 0.999f);
        prevNeckRotation = neckBone.transform.rotation;
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
        NavMeshHit navMeshHit;
        NavMesh.SamplePosition(transform.position, out navMeshHit, 10.0f, -1);
        transform.position = navMeshHit.position;

        if (!animator.GetBool("isWalking"))
        {
            return;
        }

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

    void SelectNavTarget ()
    {
        if (
            navTarget == null
            || navTarget.transform == null
            || navTarget.transform.position.x == float.PositiveInfinity
            || navTarget.transform.position.x == float.NegativeInfinity
        )
        {
            navTarget = null;
        }

        var enemyManager = GameObject.Find("GameManager").GetComponent<EnemyManager>();

        EnemyController closestAliveEnemy = null;
        var closestEnemyDistance = 10000000.0f;

        foreach (EnemyController curEnemyCtrl in enemyManager.Enemies)
        {
            if (curEnemyCtrl == null)
            {
                continue;
            }
            var enemyGO = curEnemyCtrl.gameObject;
            if (enemyGO == null)
            {
                continue;
            }
            var enemyDistance = (enemyGO.transform.position - transform.position).magnitude;
            if (enemyDistance < closestEnemyDistance)
            {
                closestAliveEnemy = curEnemyCtrl;
                closestEnemyDistance = enemyDistance;
            }
        }

        var manualNinTargetObject = GameObject.Find("NinTarget");

        if (
            closestAliveEnemy != null
            && (
                navTarget == null
                || navTarget == manualNinTargetObject
            )
        )
        {
            navTarget = closestAliveEnemy.gameObject;
            return;
        }

        if (!navTarget) { 
            navTarget = manualNinTargetObject;
        }
    }

    void Navigate()
    {
        if (isDoingFightMove)
        {
            return;
        }

        SelectNavTarget();

        doingSharpTurn = false;
        nextCornerIsDefined = false;

        Vector3 targetPosition;
        NavMeshHit navTargetNavMeshHit;
        NavMesh.SamplePosition(navTarget.transform.position, out navTargetNavMeshHit, 10.0f, -1);

        targetPosition = navTargetNavMeshHit.position;

        NavMeshPath testPath = new();
        NavMesh.CalculatePath(transform.position, targetPosition, 1, testPath);

        var distanceToTarget = (targetPosition - transform.position).magnitude;
        if (testPath.corners.Length < 2 || distanceToTarget < 1)
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
            doingSharpTurn = true;
            animator.SetBool("isWalking", false);
        }

        Quaternion initialRotQuaternion = transform.rotation;

        transform.LookAt(testPath.corners[1]);
        Quaternion rotQuaternionToNextCorner = transform.rotation;

        nextCorner = testPath.corners[1];
        nextCornerIsDefined = true;

        transform.rotation = Quaternion.Slerp(rotQuaternionToNextCorner, initialRotQuaternion, 0.995f);
    }

    void NavigateBodyMotion()
    {
        var upperBodyBone = GameObject.Find("UpperBody");

        upperBodyBone.transform.rotation = transform.rotation;
        var rotationDegToSet = 45 * Mathf.Sin(Time.time / 5);
        upperBodyBone.transform.Rotate(0, rotationDegToSet, 0);
    }

    public void handleColliderEvent(string eventType, string colliderMeshName, Collider  colArg)
    {
        var secondsSinceLastPunch = (DateTime.Now.Ticks - lastHitTime) / TimeSpan.TicksPerSecond;
        var readyToPunchGeneral = !doingSharpTurn;
        var readyToPunchBlock = readyToPunchGeneral && secondsSinceLastPunch > 8;
        var readyToPunchEnemy = readyToPunchGeneral && secondsSinceLastPunch > 8;

        if (
            colliderMeshName == "downPunchArea"
            && colArg.GetComponent<ABlockController>()
            && readyToPunchBlock
        )
        {
            animator.SetTrigger("shouldDoDownPunch");
            lastHitTime = DateTime.Now.Ticks;
            animator.SetBool("isWalking", false);
            isDoingFightMove = true;
            return;
        }
        else if (
            colliderMeshName == "leftLegLowKickArea"
            && (
                colArg.GetComponent<ABlockController>()
                || colArg.GetComponent<EnemyController>()
            )
            && readyToPunchEnemy
        )
        {
            animator.SetTrigger("shouldDoLeftLegLowKick");
            lastHitTime = DateTime.Now.Ticks;
            animator.SetBool("isWalking", false);
            isDoingFightMove = true;
            return;
        } 
        else if (
            colliderMeshName == "downPunchAreaExtended"
            && colArg.GetComponent<EnemyController>()
            && readyToPunchEnemy
        )
        {
            animator.SetTrigger("shouldDoDownPunch");
            lastHitTime = DateTime.Now.Ticks;
            animator.SetBool("isWalking", false);
            isDoingFightMove = true;
            return;
        }
    }
    void HandleDownPunchAnimEnd()
    {
        // Debug.Log("HandleDownPunchAnimEnd");

        isDoingFightMove = false;

        animator.Play("rest");

        var ankleRigidbody = leftAnkleFastCollider.GetComponent<Rigidbody>();
        /*Debug.Log("ankleRigidbody.position is " + ankleRigidbody.position);
        Debug.Log("leftAnkleFastCollider.transform.position is " + leftAnkleFastCollider.transform.position);

        Debug.Log("changing transform.position");*/
        transform.position = transform.position + transform.forward * 0.527f * transform.localScale.x;

        /*Debug.Log("ankleRigidbody.position is " + ankleRigidbody.position);
        Debug.Log("leftAnkleFastCollider.transform.position is " + leftAnkleFastCollider.transform.position);*/
    }

    void HandleFightMoveEnd()
    {
        isDoingFightMove = false;
    }

    void HandleRapidLeftLegMoveStart()
    {
        Debug.Log("LeftAnkle: MovePosition enabled");
        teleportLeftAnkleRigidbody = false;
    }

    void HandleRapidLeftLegMoveEnd()
    {
        Debug.Log("LeftAnkle: MovePosition disabled");
        teleportLeftAnkleRigidbody = true;
    }

    void HandleRapidRightArmMoveStart()
    {
        Debug.Log("RightArm: MovePosition enabled");
        teleportRightWristRigidbody = false;
    }

    void HandleRapidRightArmMoveEnd()
    {
        Debug.Log("RightArm: MovePosition disabled");
        teleportRightWristRigidbody = true;
    }

    void UpdateFight()
    {
        // var punchFrontAreaCtrl = transform.Find("PunchFrontArea").gameObject.GetComponent<PunchFrontAreaCtrl>();

        // Collision lastCollisionInfo = punchFrontAreaCtrl.lastCollisionInfo;

        // Debug.Log(lastCollisionInfo);

        /*
        Collider[] collidersInPunchArea = Physics.OverlapSphere(punchFrontAreaCollider.transform.position, punchFrontAreaCollider.radius * transform.localScale.x);

        Collider[] targetsInPunchArea = collidersInPunchArea.Where(c => {
            if (!c.GetComponent<ABlockController>())
            {
                return false;
            }
            return true;
        }).ToArray();

        var readyForNextPunch = (DateTime.Now.Ticks - lastHitTime) / TimeSpan.TicksPerSecond > 5;
        if (
            targetsInPunchArea.Length > 0
            && readyForNextPunch)
        {
            var animator = GetComponent<Animator>();
            animator.SetTrigger("shouldKick");
            lastHitTime = DateTime.Now.Ticks;
        }*/
    }

    void Update()
    {
        Navigate();

        UpdateFight();

        UpdateWalk();

        // NavigateBodyMotion();

        UpdateCameraShake();


    }

    private void FixedUpdate()
    {
        // Debug.Log("FixedUpdate");
        var ankleRigidbody = leftAnkleFastCollider.GetComponent<Rigidbody>();
        if (teleportLeftAnkleRigidbody)
        {
            // Debug.Log("changing ankleRigidbody.position from " + ankleRigidbody.position + " to " + leftAnkleBone.transform.position);
            ankleRigidbody.position = leftAnkleBone.transform.position;
            // Debug.Log("now ankleRigidbody.position is " + ankleRigidbody.position);
        }
        else
        {
            // Debug.Log("changing ankleRigidbody.position via MovePosition from " + ankleRigidbody.position + " to " + leftAnkleBone.transform.position);
            ankleRigidbody.MovePosition(leftAnkleBone.transform.position);
            // Debug.Log("now ankleRigidbody.position is " + ankleRigidbody.position);
        }

        var rightWristRigidbody = rightWristFastCollider.GetComponent<Rigidbody>();
        if (teleportRightWristRigidbody)
        {
            rightWristRigidbody.position = rightWristBone.transform.position;
        }
        else
        {
            rightWristRigidbody.MovePosition(rightWristBone.transform.position);
        }
    }

    private void LateUpdate()
    {
        UpdatePlayerTrackingSight();
    }
}
