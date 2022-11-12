using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownPunchAreaCtrl : MonoBehaviour
{
    public GameObject ninRootObject;
    public Collision lastCollisionInfo;
    NinController ninController;

    // Start is called before the first frame update
    void Start()
    {
        ninController = ninRootObject.GetComponent<NinController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("OnTriggerEnter");
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerStay(Collider colliderInfo)
    {

        ninController.handleColliderEvent("objectInDownPunchArea", colliderInfo);
        // Debug.Log("OnTriggerStay");
    }
}
