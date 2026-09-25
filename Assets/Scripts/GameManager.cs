using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Tilemaps;

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

    public float jamSpawnRate = 0.25f;
    private float jamSpawnTimer = 0f;


    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    public Transform gameCanvas;
    public float money, jarPrice = 10f;
    int numJamsDone;

    public float factorySpeed = 0.5f, tolerance = 0.5f;
    private InputAction dispenseAction, screwAction, labelAction;

    [SerializeField] GameObject jamFlowAnim;
    public Animator jamLidPutterOnner;
    public Animator jamLabeler;
    [SerializeField] Tilemap factoryGrid;
    [SerializeField] GameObject jarBuyButton;
    [SerializeField] TMPro.TextMeshProUGUI jamEndText;
    [SerializeField] Sprite jamLidSprite1, jamLabelSprite1;

    private Vector3Int[] topConveyorTileAssetsToPause = new Vector3Int[14]
    {
        new Vector3Int(-7, -3, 0),
        new Vector3Int(-7, -2, 0),
        new Vector3Int(-6, -3, 0),
        new Vector3Int(-5, -3, 0),
        new Vector3Int(-4, -3, 0),
        new Vector3Int(-3, -3, 0),
        new Vector3Int(-2, -3, 0),
        new Vector3Int(-1, -3, 0),
        new Vector3Int(0, -3, 0),
        new Vector3Int(1, -3, 0),
        new Vector3Int(2, -3, 0),
        new Vector3Int(3, -3, 0),
        new Vector3Int(4, -3, 0),
        new Vector3Int(5, -3, 0)

    };
    private Vector3Int[] bottomConveyorTileAssetsToPause = new Vector3Int[14]
    {
        new Vector3Int(-7, -9, 0),
        new Vector3Int(-6, -9, 0),
        new Vector3Int(-5, -9, 0),
        new Vector3Int(-4, -9, 0),
        new Vector3Int(-3, -9, 0),
        new Vector3Int(-2, -9, 0),
        new Vector3Int(-1, -9, 0),
        new Vector3Int(0, -9, 0),
        new Vector3Int(1, -9, 0),
        new Vector3Int(2, -9, 0),
        new Vector3Int(3, -9, 0),
        new Vector3Int(4, -9, 0),
        new Vector3Int(5, -9, 0),
        new Vector3Int(6, -9, 0),

    };
    public bool topPaused = false, bottomPaused = false;

    AnimatedTile claw;

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

        jamLidPutterOnner.speed = 0;
        jamLabeler.speed = 0;

        moneyText.text = "$0";
        
        claw = factoryGrid.GetTile(new Vector3Int(-7, -2, 0)) as AnimatedTile;
        claw.m_MinSpeed = claw.m_MaxSpeed = 3;

        factoryGrid.RefreshAllTiles();

        StartCoroutine(CheckWhenMusicEnds());
    }

    private void Update()
    {
        if (dispenseAction.WasPressedThisFrame() && !jamFlowAnim.activeSelf)
        {
            StartCoroutine(DispenseJam());
        }
        if (screwAction.WasPressedThisFrame() && jamLidPutterOnner.speed == 0)
        {
            StartCoroutine(PutOnLid());
        }
        if (labelAction.WasPressedThisFrame() && jamLabeler.speed == 0)
        {
            StartCoroutine(PutOnLabel());
        }

        foreach (Jam jam in jams)
        {
            if (jam.startMovingDelay > 0 || (jam.lastNodePassed == 0 && topPaused) || (jam.lastNodePassed == 3 && bottomPaused)) continue;

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

        numJamsDone += jams.RemoveAll(jam => jam.dstAlongPathNormalized >= 1);

        if (!topPaused && !bottomPaused)
        {
            jamSpawnTimer -= Time.deltaTime;
            if (jamSpawnTimer <= 0)
            {
                GameObject newJam = Instantiate(jamPrefab, jamParent);
                newJam.transform.position = jamPath.transform.position;

                jams.Add(newJam.GetComponent<Jam>());

                jamSpawnTimer = 1f / jamSpawnRate;
            }
        }
    }

    IEnumerator CheckWhenMusicEnds()
    {
        // Wait until isPlaying becomes false
        yield return new WaitUntil(() => !GetComponent<AudioSource>().isPlaying);

        jamEndText.transform.parent.gameObject.SetActive(true);
        jamEndText.text = "You sold " + numJamsDone.ToString() + " jars of jam!";
        gameObject.SetActive(false);
    }

    public void BuyJars()
    {
        if (money < jarPrice) return;

        money -= jarPrice;
        moneyText.text = "$" + money.ToString("F2");
        jamSpawnRate *= 2;
        jarPrice *= 2;

        for (int i = 0; i < conveyorSpeeds.Length; i++) conveyorSpeeds[i] *= 1.5f;

        if (jamSpawnRate == 1) factorySpeed /= 2f;
        if (jamSpawnRate == 4) jarBuyButton.SetActive(false);

        claw.m_MinSpeed *= 2;
        claw.m_MaxSpeed *= 2;
    }

    public void AddScore(float score)
    {
        money += score;
        moneyText.text = "$" + money.ToString("F2");
    }

    void PauseFactory(bool top)
    {
        if (top)
        {
            ChangeConveyorAnim(topConveyorTileAssetsToPause, TileAnimationFlags.PauseAnimation);
            topPaused = true;
        }
        else
        {
            ChangeConveyorAnim(bottomConveyorTileAssetsToPause, TileAnimationFlags.PauseAnimation);
            bottomPaused = true;
        }
    }
    void ResumeFactory(bool top)
    {
        if (top)
        {
            ChangeConveyorAnim(topConveyorTileAssetsToPause, TileAnimationFlags.None);
            topPaused = false;
        }
        else
        {
            ChangeConveyorAnim(bottomConveyorTileAssetsToPause, TileAnimationFlags.None);
            bottomPaused = false;
        }
    }
    void ChangeConveyorAnim(Vector3Int[] coords, TileAnimationFlags flag)
    {
        foreach (Vector3Int coord in coords)
        {
            factoryGrid.SetTileAnimationFlags(coord, flag);
            print("Paused " + coord);
        }
    }

    IEnumerator DispenseJam()
    {
        PauseFactory(true);

        jamFlowAnim.SetActive(true);
        jamFlowAnim.GetComponent<AudioSource>().Play();

        Jam jam = LookForJamAtX(0, jamFlowAnim.transform.position.x);
        if (jam != null) StartCoroutine(jam.FillWithJam());

        yield return new WaitForSeconds(factorySpeed);

        jamFlowAnim.SetActive(false);
        ResumeFactory(true);
    }
    IEnumerator PutOnLid()
    {
        PauseFactory(true);
        jamLidPutterOnner.speed = factorySpeed * 2;

        Jam jam = LookForJamAtX(0, jamLidPutterOnner.transform.position.x);
        if (jam != null) StartCoroutine(jam.ScrewLid());

        yield return new WaitForSeconds(factorySpeed);


        ResumeFactory(true);

        yield return new WaitForSeconds(0.6f);
        jamLidPutterOnner.speed = 0;
    }
    IEnumerator PutOnLabel()
    {
        PauseFactory(false);
        jamLabeler.speed = factorySpeed * 2;

        Jam jam = LookForJamAtX(1, jamLabeler.transform.position.x);
        if (jam != null) StartCoroutine(jam.AddLabel());

        yield return new WaitForSeconds(factorySpeed);

        ResumeFactory(false);

        yield return new WaitForSeconds(0.4f);

        jamLabeler.speed = 0;
    }

    Jam LookForJamAtX(int level, float x)
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

                return jam;
            }
        }
        return null;
    }
}
