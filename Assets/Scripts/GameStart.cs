using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] GameObject gameManager;

    private void Start()
    {
        gameManager.GetComponent<GameManager>().jamLabeler.speed = 0;
        gameManager.GetComponent<GameManager>().jamLidPutterOnner.speed = 0;
    }

    public void StartGame()
    {
        gameManager.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
