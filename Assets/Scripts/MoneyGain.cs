using UnityEngine;
using UnityEngine.UI;

public class MoneyGain : MonoBehaviour
{
    float fadeTimer = 2f;

    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] Image img;

    Vector3 dir;

    private void Awake()
    {
        dir = new Vector3(Random.value, Random.value).normalized;
    }

    void Update()
    {
        transform.Translate(dir *  Time.deltaTime);

        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;

            Color tCol = text.color;
            tCol.a = fadeTimer / 2;
            text.color = tCol;

            Color iCol = img.color;
            iCol.a = fadeTimer / 2;
            img.color = iCol;

            if (fadeTimer < 0) Destroy(gameObject);
        }
    }

    public void SetMoney(float money)
    {
        text.text = "$"+money.ToString("F2");
    }
}
