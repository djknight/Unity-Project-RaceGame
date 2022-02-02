using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointChecker : MonoBehaviour
{
    public CarController theCar;
private void OnTriggerEnter(Collider other)

 {
     //the Tag CheckPoint
  if(other.tag == "Checkpoint")
  {
     // Debug.Log("Hit cp " + other.GetComponent<CheckPoint>().cpNumber);

     theCar.checkpointHit(other.GetComponent<CheckPoint>().cpNumber);
  }  
 }


}
