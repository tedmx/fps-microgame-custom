using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMeshCtrl : MonoBehaviour
{
    public GameObject ninRootObject;
    public Collision lastCollisionInfo;
    NinController ninController;
    public string thisTriggerCodeName = "Untitled Trigger";

    // Start is called before the first frame update
    void Start()
    {
        ninController = ninRootObject.GetComponent<NinController>();
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerStay(Collider colliderInfo)
    {

        ninController.handleColliderEvent("objectInTriggerMesh", thisTriggerCodeName, colliderInfo);
    }
}
