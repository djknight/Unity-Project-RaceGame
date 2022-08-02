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

    //car sounds
    public AudioSource engineSound, skidSound;
    public float skidFadeSpeed;

    //checkpoints laps
    private int nextCheckpoint;
    public int currentLap;

    //laptimes
    public float lapTime, bestLapTime;

    //AI Cars
    public bool isAI;

    public int currentTarget;

    //AI storing the postion Ai cars are chasing points
    private UnityEngine.Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f, aiReachPointRange = 5f, aiPointVariance = 3f, aiMaxTurn = 15f;
    private float aiSpeedInput, aiSpeedMod;

    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;

        dragOnGround = theRB.drag;

        if (isAI)
        {
            targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            RandomiseAITarget();

            aiSpeedMod = Random.Range(.8f, 1.1f);
        }

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;

    }

    // Update is called once per frame
    void Update()
    {
        //laptime & sec count
        lapTime += Time.deltaTime;


        //if AI, don't do anything
        if (!isAI)
        {
            //converts  ts /timespant to lapTime then to the laptime in the UI
            var ts = System.TimeSpan.FromSeconds(lapTime);
            UIManager.instance.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);


            //forward and Reverse
            speedInput = 0f;
            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;
            }
            //turn input
            turnInput = Input.GetAxis("Horizontal");


            //old code
            // /* if ( grounded && Input.GetAxis("Vertical") != 0)
            //   {
            //       transform.rotation = UnityEngine.Quaternion.Euler (transform.rotation.eulerAngles + new UnityEngine.Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign (speedInput) *(theRB .velocity.magnitude / maxSpeed) , 0f));
            //  } */

            // car AI ceckpoints positions & check if reached
        }
        else
        {
            targetPoint.y = transform.position.y;

            if (UnityEngine.Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
            {
                SetNextAITarget();

                //no longer neaded reapeatedly setting the target point

                // currentTarget++;
                // if (currentTarget >= RaceManager.instance.allCheckpoints.Length)
                // {
                //     currentTarget = 0;
                // }
                // targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                // RandomiseAITarget();

            }

            //odd code here (AI) (turning) (acelerate) (direction) if changed also affects player car ????

            // vec3 target direction

            UnityEngine.Vector3 TargetDIR = targetPoint - transform.position;
            float angle = UnityEngine.Vector3.Angle(TargetDIR, transform.forward);

            //position of the car Ai is facing towards
            UnityEngine.Vector3 localPos = transform.InverseTransformPoint(targetPoint);
            if (localPos.x < 0f)
            {
                angle = -angle;
            }

            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);

            //accelerate/angle/direction
            if (Mathf.Abs(angle) < aiMaxTurn)
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
            }
            else
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
            }


            
            speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
        }



        //wheel Turn Left Right
        leftFrontWheel.localRotation = UnityEngine.Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = UnityEngine.Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);

        //transform.position = theRB.position;


        //control partical emissions
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }

        if (theRB.velocity.magnitude <= .5f)
        {
            emissionRate = 0;
        }

        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;

            emissionModule.rateOverTime = emissionRate;
        }
        //sound of pitch to speed of the car
        if (engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) * 2f);
        }

        //skidSound

        if (skidSound != null)
        {
            if (grounded && Mathf.Abs(turnInput) > .8f && theRB.velocity.magnitude >= 7f)

            {
                skidSound.volume = .200f;
            }
            else
            {
                skidSound.volume = Mathf.MoveTowards(skidSound.volume, 0f, skidFadeSpeed * Time.deltaTime);
            }
        }

        //old code
        // if (skidSound != null)
        // {
        //     if (Mathf.Abs(turnInput) > 0.9f)
        //     {
        //         skidSound.volume = 1f;
        //     }
        //     else
        //     {
        //         skidSound.volume = Mathf.MoveTowards(skidSound.volume, 0f, skidFadeSpeed * Time.deltaTime);
        //     }
        //     if (theRB.velocity.magnitude <= 0.9f)
        //     // grounded = false;

        //     {
        //         skidSound.volume = 0;
        //     }
        // }
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

        if (Physics.Raycast(groundedRayPoint2.position, -transform.up, out hit, groundRayLength, WhatIsGround))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal) / 2f;
        }

        //when on ground rotate rotate to match the normal (green up and down is the Normal)

        if (grounded)
        {
            transform.rotation = UnityEngine.Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        //speed and Accelerate fixed update

        if (grounded)
        {
            theRB.drag = dragOnGround;
            theRB.AddForce(transform.forward * speedInput * 1000f);
        }
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-UnityEngine.Vector3.up * gravityMod * 100f);
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }
        //Debug.Log(theRB.velocity.magnitude);

        transform.position = theRB.position;

        if (grounded && speedInput != 0)
        {
            transform.rotation = UnityEngine.Quaternion.Euler(transform.rotation.eulerAngles + new UnityEngine.Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
        }
    }
    //checkpoint hit
    public void checkpointHit(int cpNumber)
    {
        if (cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            //racemanager script call
            if (nextCheckpoint == RaceManager.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }

            if (isAI)
            {
                if (cpNumber == currentTarget)
                {
                    SetNextAITarget();
                }

            }
        }

        //Debug.Log(cpNumber);
    }
    //ai if car hits waypoint then go to next waypoint if skipped the mark
    public void SetNextAITarget()
    {
        currentTarget++;
        if (currentTarget == RaceManager.instance.allCheckpoints.Length)
        {
            currentTarget = 0;
        }
        targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
        RandomiseAITarget();
    }
    //lap UI diplay controll
    public void LapCompleted()
    {
        currentLap++;

        if (lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }
        lapTime = 0f;
        //bestlaptime aded converted into UI
        if (!isAI)
        {
            var ts = System.TimeSpan.FromSeconds(bestLapTime);
            UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

            // calling UI  to manaage  lap count to text  current lap + lap count /total laps for canvas (pain in the .... dont lose this code again. )
            UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        }
    }

    public void RandomiseAITarget()
    {
        targetPoint += new UnityEngine.Vector3(Random.Range(-aiPointVariance, aiPointVariance), 0f, Random.Range(-aiPointVariance, aiPointVariance));

        // ai eazy med hard difficulty
        // //randomise the AI
        // int random = Random.Range(0, 3);
        // if (random == 0)
        // {
        //     aiType = AIType.Easy;
        // }
        // else if (random == 1)
        // {
        //     aiType = AIType.Medium;
        // }
        // else if (random == 2)
        // {
        //     aiType = AIType.Hard;
        // }
    }

}
