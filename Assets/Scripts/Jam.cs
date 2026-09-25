using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jam : MonoBehaviour
{
    [SerializeField] GameObject moneyGainedLabel, emptyLid;

    public float dstAlongPathNormalized = 0f;
    public int lastNodePassed = 0;

    public float timeBeforeSortingFront, startMovingDelay;

    private Vector3 clawGrabUpPos = new Vector3(-6.49373245f, -0.764419317f, 0);

    int factorySteps = 3;
    public List<float> stepQuality = new();

    int costumeID = 0;

    private void Awake()
    {
    }
    private void Start()
    {
        timeBeforeSortingFront = startMovingDelay = 1f / GameManager.instance.jamSpawnRate / 2f;
    }
    private void Update()
    {
        if (timeBeforeSortingFront > 0 && !GameManager.instance.topPaused)
        {
            timeBeforeSortingFront -= Time.deltaTime;
            transform.position = Vector3.Lerp(GameManager.instance.jamPath.transform.position, clawGrabUpPos, 1 - timeBeforeSortingFront * 2);

            if (timeBeforeSortingFront <= 0) transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
        else if (startMovingDelay > 0 && !GameManager.instance.topPaused)
        {
            //Move down with claw onto conveyor
            startMovingDelay -= Time.deltaTime;
            transform.position = Vector3.Lerp(clawGrabUpPos, GameManager.instance.jamPath.transform.position, 1 - startMovingDelay * 2);
        }
    }


    public void JamFinishedConveyorTrip()
    {
        float score = 0;
        foreach (float quality in stepQuality)
        {
            score += quality;
        }
        score -= (factorySteps - stepQuality.Count) / 5f;

        GameObject scoreObj = Instantiate(moneyGainedLabel, GameManager.instance.gameCanvas);
        scoreObj.transform.position = gameObject.transform.position + Vector3.right * 3;

        scoreObj.GetComponent<MoneyGain>().SetMoney(score);
        GameManager.instance.AddScore(score);

        Destroy(gameObject);
    }

    void NextCostume()
    {
        transform.GetChild(costumeID).gameObject.SetActive(false);
        costumeID++;
        transform.GetChild(costumeID).gameObject.SetActive(true);
    }

    public IEnumerator FillWithJam()
    {
        yield return new WaitForSeconds(0.15f);
        NextCostume();

        yield return new WaitForSeconds(0.15f);
        NextCostume();

        yield return new WaitForSeconds(0.15f);
        NextCostume();

        yield return new WaitForSeconds(0.15f);
        NextCostume();
    }

    public IEnumerator ScrewLid()
    {
        yield return new WaitForSeconds(0.4f);

        if (costumeID > 0)
        {
            NextCostume();
            yield return new WaitForSeconds(0.05f);
            NextCostume();
            yield return new WaitForSeconds(0.05f);
            NextCostume();
            yield return new WaitForSeconds(0.05f);
        }
        else emptyLid.SetActive(true);
    }


    public IEnumerator AddLabel()
    {
        yield return new WaitForSeconds(0.4f);

        transform.GetChild(8).gameObject.SetActive(true);
    }
}
