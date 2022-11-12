using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using UnityEngine;

public class TestEnemyCtrl : MonoBehaviour
{

    Vector3 prevPosition;

    // Start is called before the first frame update
    void Start()
    {
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var positionDelta = transform.position - prevPosition;
        var positionDeltaMagnitude = positionDelta.magnitude;
        if (positionDeltaMagnitude > 0.2f) {
            var enemyMobileScriptComp = this.GetComponent<EnemyMobile>();
            enemyMobileScriptComp.enabled = false;
        }
        prevPosition = transform.position;
    }
}
