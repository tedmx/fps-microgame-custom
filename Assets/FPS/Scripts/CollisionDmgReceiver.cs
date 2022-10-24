using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class CollisionDmgReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null)
        {
            // return;
        }
        bool colliderIsABlock = true || collision.rigidbody.GetComponent<Unity.FPS.AI.ABlockController>() != null;
        if (colliderIsABlock && collision.impulse.magnitude > 5)
        {
            Debug.Log(collision.impulse.magnitude);
            Debug.Log("oops this hurts B");
            Health thisHealth = GetComponent<Health>();
            thisHealth.TakeDamage(4000, gameObject);

        }
    }
}
