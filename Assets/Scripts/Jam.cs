using System.Collections;
using UnityEngine;

public class Jam : MonoBehaviour
{
    public float dstAlongPathNormalized = 0f;
    public int lastNodePassed = 0;


    public void JamFinishedConveyorTrip()
    {
        Destroy(gameObject);
    }
}
