using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // cam follow target
    public CarController target;
    private Vector3 offsetDir;

    //cam zoom / distance
    public float minDistance,maxDistance;
    private float activeDistance;


    // Start is called before the first frame update
    void Start()
    {
        offsetDir = transform.position - target.transform.position;

        //Distance start
        activeDistance = minDistance;

        offsetDir.Normalize();

    }

    // Update is called once per frame
    void Update()
    {
        //Dis per Frame speed of car frame per speed and dist

        activeDistance = minDistance + ((maxDistance - minDistance) * (target.theRB.velocity.magnitude / target.maxSpeed));


        //cam pos
        transform.position = target.transform.position + (offsetDir * activeDistance);
    }
}
