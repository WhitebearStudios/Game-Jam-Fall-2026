using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Jam> jams;
    public GameObject jamPrefab;
    public Transform jamParent;

    public static GameManager instance;
    //The path alternates conveyors and slides in between nodes. Start with a conveyor
    //Slides and conveyors also shouldn't be super short
    public GameObject jamPath;
    private Spline jamSpline;
    public float[] conveyorSpeeds;
    [SerializeField] float slideSpeed;
    public Vector3 jamPosOffset;
    private float pathLength;

    public float jamSpawnRate = 1;
    private float jamSpawnTimer = 0f;


    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    public int money;





    private void Awake()
    {
        instance = this;

        jamSpawnTimer = 1f / jamSpawnRate;
    }

    private void Start()
    {
        pathLength = jamPath.GetComponent<SplineContainer>().CalculateLength();
        jamSpline = jamPath.GetComponent<SplineContainer>().Spline;
    }

    private void Update()
    {
        foreach (Jam jam in jams)
        {

            float mySpeed = slideSpeed;

            //Find which part of the path the jam is on
            if(jam.lastNodePassed % 3 == 0)
            {
                //On a conveyor.
                mySpeed = conveyorSpeeds[jam.lastNodePassed / 3];

                
            }
            jam.dstAlongPathNormalized += (mySpeed * Time.deltaTime) / pathLength;
            float nextNodeProgress = jamSpline.ConvertIndexUnit(jam.lastNodePassed + 1, PathIndexUnit.Knot, PathIndexUnit.Normalized);

            if (jam.dstAlongPathNormalized > nextNodeProgress) jam.lastNodePassed++;

            float3 newJamSplinePos = jamSpline.EvaluatePosition(jam.dstAlongPathNormalized);
            jam.transform.position = new Vector3(newJamSplinePos.x, newJamSplinePos.y, newJamSplinePos.z) + jamPath.transform.position + jamPosOffset;


            if (jam.dstAlongPathNormalized >= 1)
            {
                //TODO: increase score or money
                jam.JamFinishedConveyorTrip();
            }

            money++;
            moneyText.text = "$" + money.ToString();
        }

        jams.RemoveAll(jam => jam.dstAlongPathNormalized >= 1);

        jamSpawnTimer -= Time.deltaTime;
        if (jamSpawnTimer <= 0)
        {
            GameObject newJam = Instantiate(jamPrefab, jamParent);
            newJam.transform.position = jamSpline.EvaluatePosition(0);

            jams.Add(newJam.GetComponent<Jam>());

            jamSpawnTimer = 1f / jamSpawnRate;
        }
    }
}
