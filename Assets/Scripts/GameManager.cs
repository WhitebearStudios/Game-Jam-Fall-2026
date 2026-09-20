using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public Jam[] jams;

    public static GameManager instance;
    public static SplineContainer jamPath;
}
