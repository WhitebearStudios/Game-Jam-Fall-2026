using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Jam> jams;
    public GameObject jamPrefab;

    public static GameManager instance;
    //The path alternates conveyors and slides in between nodes. Start with a conveyor
    //Slides and conveyors also shouldn't be super short
    public GameObject jamPath;
    private Spline jamSpline;
    public float[] conveyorSpeeds;
    [SerializeField] float slideSpeed;
    public Vector3 jamPosOffset;

    private float pathLength;

    private void Start()
    {
        pathLength = jamPath.GetComponent<SplineContainer>().CalculateLength();
        jamSpline = jamPath.GetComponent<SplineContainer>().Spline;
    }

    private void Update()
    {
        foreach (Jam jam in jams)
        {
            if (jam.dstAlongPathNormalized >= 1) continue;
            float mySpeed = slideSpeed;

            //Find which part of the path the jam is on
            if(jam.lastNodePassed % 2 == 0)
            {
                //On a conveyor.
                mySpeed = conveyorSpeeds[jam.lastNodePassed / 2];

                
            }
            jam.dstAlongPathNormalized += (mySpeed * Time.deltaTime) / pathLength;
            float nextNodeProgress = jamSpline.ConvertIndexUnit(jam.lastNodePassed + 1, PathIndexUnit.Knot, PathIndexUnit.Normalized);

            if (jam.dstAlongPathNormalized > nextNodeProgress) jam.lastNodePassed++;

            float3 newJamSplinePos = jamSpline.EvaluatePosition(jam.dstAlongPathNormalized);
            jam.transform.position = new Vector3(newJamSplinePos.x, newJamSplinePos.y, newJamSplinePos.z) + jamPath.transform.position + jamPosOffset;
        }
    }
}
