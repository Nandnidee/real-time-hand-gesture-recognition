using UnityEngine;
using TMPro;
using System.Collections;

public class GestureUI : MonoBehaviour
{
    public TextMeshProUGUI gestureText;
    public TextMeshProUGUI counterText;
    public GameObject floatingTextPrefab;

    int likeCount;
    int dislikeCount;
    int dilCount;
    int celebrateCount;

    void Start()
    {
        likeCount = PlayerPrefs.GetInt("like", 0);
        dislikeCount = PlayerPrefs.GetInt("dislike", 0);
        dilCount = PlayerPrefs.GetInt("dil", 0);
        celebrateCount = PlayerPrefs.GetInt("celebrate", 0);

        UpdateCounterText();
    }

    public void ShowGesture(string name, Vector3 worldPos)
    {
        gestureText.text = name.ToUpper();
        SetColor(name);

        StopAllCoroutines();
        StartCoroutine(PopAnimation());
        StartCoroutine(FadeText());

        SpawnFloatingText(worldPos, "+1");
    }

    public void IncreaseCounter(string name)
    {
        if (name == "like") likeCount++;
        else if (name == "dislike") dislikeCount++;
        else if (name == "dil") dilCount++;
        else if (name == "celebrate") celebrateCount++;

        PlayerPrefs.SetInt("like", likeCount);
        PlayerPrefs.SetInt("dislike", dislikeCount);
        PlayerPrefs.SetInt("dil", dilCount);
        PlayerPrefs.SetInt("celebrate", celebrateCount);

        UpdateCounterText();
    }

    void UpdateCounterText()
    {
        counterText.text =
            "Like: " + likeCount +
            "\nDislike: " + dislikeCount +
            "\nDil: " + dilCount +
            "\nCelebrate: " + celebrateCount;
    }

    void SetColor(string name)
    {
        if (name == "like") gestureText.color = Color.green;
        else if (name == "dislike") gestureText.color = Color.red;
        else if (name == "dil") gestureText.color = Color.magenta;
        else if (name == "celebrate") gestureText.color = Color.yellow;
    }

    IEnumerator FadeText()
    {
        gestureText.alpha = 1f;
        yield return new WaitForSeconds(1.2f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            gestureText.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
    }

    IEnumerator PopAnimation()
    {
        gestureText.transform.localScale = Vector3.one * 0.6f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            gestureText.transform.localScale = Vector3.Lerp(
                Vector3.one * 0.6f,
                Vector3.one,
                t
            );
            yield return null;
        }
    }

    void SpawnFloatingText(Vector3 worldPos, string text)
    {
        if (floatingTextPrefab == null) return;

        GameObject obj = Instantiate(floatingTextPrefab, worldPos, Quaternion.identity);
        obj.GetComponent<TextMeshPro>().text = text;
        Destroy(obj, 1.5f);
    }
}