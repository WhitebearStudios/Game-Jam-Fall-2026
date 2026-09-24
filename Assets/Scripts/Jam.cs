using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jam : MonoBehaviour
{
    [SerializeField] GameObject moneyGainedLabel;

    public float dstAlongPathNormalized = 0f;
    public int lastNodePassed = 0;

    public float timeBeforeSortingFront, startMovingDelay;

    SpriteRenderer spriteRenderer;

    private Vector3 clawGrabUpPos = new Vector3(-6.49373245f, -0.764419317f, 0);

    int factorySteps = 1;
    public List<float> stepQuality = new();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        timeBeforeSortingFront = startMovingDelay = 1f / GameManager.instance.jamSpawnRate / 2f;
    }
    private void Update()
    {
        if (timeBeforeSortingFront > 0)
        {
            timeBeforeSortingFront -= Time.deltaTime;
            transform.position = Vector3.Lerp(GameManager.instance.jamPath.transform.position, clawGrabUpPos, 1 - timeBeforeSortingFront * 2);

            if (timeBeforeSortingFront <= 0) spriteRenderer.sortingOrder = 10;
        }
        else if (startMovingDelay > 0)
        {
            //Move down with claw onto conveyor
            startMovingDelay -= Time.deltaTime;
            transform.position = Vector3.Lerp(clawGrabUpPos, GameManager.instance.jamPath.transform.position, 1 - startMovingDelay * 2);
        }
    }


    public void JamFinishedConveyorTrip()
    {
        int score = 0;
        foreach (float quality in stepQuality)
        {
            score += Mathf.RoundToInt(quality * 100);
        }
        score -= (factorySteps - stepQuality.Count) * 50;

        GameObject scoreObj = Instantiate(moneyGainedLabel, GameManager.instance.gameCanvas);
        scoreObj.transform.position = gameObject.transform.position + Vector3.right * 3;

        scoreObj.GetComponent<MoneyGain>().SetMoney(score);
        GameManager.instance.AddScore(score);

        Destroy(gameObject);
    }
}
