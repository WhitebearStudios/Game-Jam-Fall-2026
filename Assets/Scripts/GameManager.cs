using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

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
    public Transform gameCanvas;
    public int money;


    public float factorySpeed = 0.5f, tolerance = 0.5f;
    private InputAction dispenseAction, screwAction, labelAction;

    [SerializeField] GameObject jamFlowAnim;
    [SerializeField] GameObject jamLidPutterOnner;
    [SerializeField] GameObject jamLabeler;


    private void Awake()
    {
        instance = this;

        jamSpawnTimer = 1f / jamSpawnRate;

        dispenseAction = InputSystem.actions.FindAction("Dispense");
        screwAction = InputSystem.actions.FindAction("Screw");
        labelAction = InputSystem.actions.FindAction("Label");
    }

    private void Start()
    {
        pathLength = jamPath.GetComponent<SplineContainer>().CalculateLength();
        jamSpline = jamPath.GetComponent<SplineContainer>().Spline;

        jamLidPutterOnner.GetComponent<Animator>().StopPlayback();
        jamLabeler.GetComponent<Animator>().StopPlayback();

        moneyText.text = "$0";
    }

    private void Update()
    {
        if (dispenseAction.WasPressedThisFrame())
        {
            StartCoroutine(DispenseJam());
        }
        if (screwAction.WasPressedThisFrame())
        {
            
        }
        if (labelAction.WasPressedThisFrame())
        {
            
        }

        foreach (Jam jam in jams)
        {
            if (jam.startMovingDelay > 0) continue;

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
        }

        jams.RemoveAll(jam => jam.dstAlongPathNormalized >= 1);

        jamSpawnTimer -= Time.deltaTime;
        if (jamSpawnTimer <= 0)
        {
            GameObject newJam = Instantiate(jamPrefab, jamParent);
            newJam.transform.position = jamPath.transform.position;

            jams.Add(newJam.GetComponent<Jam>());

            jamSpawnTimer = 1f / jamSpawnRate;
        }
    }

    public void AddScore(int score)
    {
        money += score;
        moneyText.text = "$" + money.ToString();
    }

    IEnumerator DispenseJam()
    {
        jamFlowAnim.SetActive(true);
        jamFlowAnim.GetComponent<AudioSource>().Play();

        LookForJamAtX(0, jamFlowAnim.transform.position.x);

        yield return new WaitForSeconds(factorySpeed);

        jamFlowAnim.SetActive(false);
    }

    void LookForJamAtX(int level, float x)
    {
        foreach (Jam jam in jams)
        {
            int jamLevel = Mathf.FloorToInt(jam.lastNodePassed / 3);
            if (level != jamLevel) continue;

            float xDiff = Mathf.Abs(jam.transform.position.x - x);

            if (xDiff < tolerance)
            {
                float quality = 1 - xDiff / tolerance;
                print("Quality: " + quality);
                jam.stepQuality.Add(quality);
            }
        }
    }
}
