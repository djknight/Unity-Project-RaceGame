using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public CheckPoint[] allCheckpoints;

    public int totalLaps;

    public CarController playerCar;
    public List <CarController> allAICars = new List<CarController>();



    private void Awake() {
        instance = this;
    }



    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].cpNumber = i; 
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
