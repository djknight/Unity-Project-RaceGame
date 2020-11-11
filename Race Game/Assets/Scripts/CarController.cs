using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Rigidbody theRB;

    public float maxSpeed;

    //forward and Reverse floats
    public float forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;

    //turn Left and right floats
    public float turnStrength = 180f;
    private float turnInput;

    //Ground
    private bool grounded;
    public Transform groundedRayPoint, groundedRayPoint2;
    public LayerMask WhatIsGround;
    public float groundRayLength = .75f;
    private float dragOnGround;
    public float gravityMod = 10f;


    //wheel turn left right
    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    //Particle's (Smoke dustTrail turning)
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;


    
    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;
        dragOnGround = theRB.drag;
    }

    // Update is called once per frame
    void Update()
    {
        //forward and Reverse
        speedInput = 0f;
        if(Input.GetAxis("Vertical") > 0 )
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        } else if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }
        //turn input
        turnInput = Input.GetAxis("Horizontal");

        if ( grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = UnityEngine.Quaternion.Euler (transform.rotation.eulerAngles + new UnityEngine.Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign (speedInput) *(theRB .velocity.magnitude / maxSpeed) , 0f));
        }

        //wheel Turn Left Right
        leftFrontWheel.localRotation = UnityEngine.Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn)-180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = UnityEngine.Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);

        transform.position = theRB.position;


        //control partical emissions
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0))) 
        {
            emissionRate = maxEmission;
        }
        
        if(theRB.velocity.magnitude <= .5f)
        {
            emissionRate = 0;
        }

        for(int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;

            emissionModule.rateOverTime = emissionRate;
        }


    }

    private void FixedUpdate()
    {

        //ground fixed update

        grounded = false;
        UnityEngine.Vector3 normalTarget = UnityEngine.Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(groundedRayPoint.position, -transform.up, out hit, groundRayLength, WhatIsGround)) 
        {
            grounded = true;
            normalTarget = hit.normal;
        }

        if(Physics.Raycast(groundedRayPoint2.position, -transform.up, out hit, groundRayLength, WhatIsGround))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal) / 2f;
        }
      
        //when on ground rotate rotate to match the normal (green up and down is the Normal)

        if(grounded)
        {
            transform.rotation = UnityEngine.Quaternion.FromToRotation(transform.up, normalTarget) *transform.rotation;
        }



        //speed and Accelerate fixed update

        if (grounded)
        {
            theRB.drag = dragOnGround;
            theRB.AddForce(transform.forward * speedInput * 1000f);
        } else
        {
            theRB.drag = .1f;
            theRB.AddForce(-UnityEngine.Vector3.up * gravityMod * 100f);
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }
        Debug.Log(theRB.velocity.magnitude);
    }


}
