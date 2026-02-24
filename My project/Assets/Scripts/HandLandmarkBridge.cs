using UnityEngine;
using System.Collections;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Mediapipe.Tasks.Vision.HandLandmarker;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
public class HandLandmarkBridge : MonoBehaviour
{
    public HandLandmarkerRunner runner;
    public GestureInference gestureInference;
    public GestureUI gestureUI;
    public GameObject heartEffect;
    public GameObject likeEffect;
    public GameObject dislikeEffect;
    public GameObject confettiEffect;
    public GameObject balloonPrefab;

    public AudioClip dilSound;
    public AudioClip likeSound;
    public AudioClip dislikeSound;
    public AudioClip celebrateSound;

    private AudioSource audioSource;

    int stablePrediction = -1;
    int stabilityCounter = 0;
    int stabilityThreshold = 15;

    bool isPlaying = false;

    const float EFFECT_DURATION = 3f;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

#if UNITY_ANDROID && !UNITY_EDITOR
    if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
    {
        Permission.RequestUserPermission(Permission.Camera);
    }
#endif
    }

    void Update()
    {
        if (runner == null || gestureInference == null) return;

        var result = runner.LatestResult;
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
            return;

        float[] landmarks = new float[126];
        int handCount = Mathf.Min(result.handLandmarks.Count, 2);

        for (int i = 0; i < handCount; i++)
        {
            var normalizedLandmarks = result.handLandmarks[i];
            if (normalizedLandmarks.landmarks == null ||
                normalizedLandmarks.landmarks.Count < 21)
                continue;

            int baseIndex = i * 63;

            for (int j = 0; j < 21; j++)
            {
                landmarks[baseIndex + j * 3 + 0] = normalizedLandmarks.landmarks[j].x;
                landmarks[baseIndex + j * 3 + 1] = normalizedLandmarks.landmarks[j].y;
                landmarks[baseIndex + j * 3 + 2] = normalizedLandmarks.landmarks[j].z;
            }
        }

        int predicted = gestureInference.Predict(landmarks);

        if (predicted == stablePrediction)
            stabilityCounter++;
        else
        {
            stablePrediction = predicted;
            stabilityCounter = 0;
        }

        if (!isPlaying && stabilityCounter > stabilityThreshold)
        {
            StartCoroutine(HandleGesture(stablePrediction));
            stabilityCounter = 0;
        }
    }

    IEnumerator HandleGesture(int id)
    {
        isPlaying = true;

        Vector3 boundaryPos = GetHandBoundaryPosition();

        AudioClip clipToPlay = null;

        if (id == 0 && heartEffect != null)
        {
            SpawnEffect(heartEffect, boundaryPos);
            clipToPlay = dilSound;

            gestureUI?.ShowGesture("dil", boundaryPos);
            gestureUI?.IncreaseCounter("dil");
        }
        else if (id == 1 && dislikeEffect != null)
        {
            SpawnEffect(dislikeEffect, boundaryPos);
            clipToPlay = dislikeSound;

            gestureUI?.ShowGesture("dislike", boundaryPos);
            gestureUI?.IncreaseCounter("dislike");
        }
        else if (id == 2 && likeEffect != null)
        {
            SpawnEffect(likeEffect, boundaryPos);
            clipToPlay = likeSound;

            gestureUI?.ShowGesture("like", boundaryPos);
            gestureUI?.IncreaseCounter("like");
        }
        else if (id == 3)
        {
            ShowCelebrateEffect();
            clipToPlay = celebrateSound;

            gestureUI?.ShowGesture("celebrate", boundaryPos);
            gestureUI?.IncreaseCounter("celebrate");
        }

        PlayLoopingSoundForDuration(clipToPlay);

        yield return new WaitForSeconds(EFFECT_DURATION);

        audioSource.Stop();
        isPlaying = false;
    }

    void PlayLoopingSoundForDuration(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;   // will repeat automatically
        audioSource.Play();
    }

    Vector3 GetHandBoundaryPosition()
    {
        var hand = runner.LatestResult.handLandmarks[0];

        float minX = 1f, maxX = 0f;
        float minY = 1f, maxY = 0f;

        foreach (var lm in hand.landmarks)
        {
            if (lm.x < minX) minX = lm.x;
            if (lm.x > maxX) maxX = lm.x;
            if (lm.y < minY) minY = lm.y;
            if (lm.y > maxY) maxY = lm.y;
        }

        int edge = Random.Range(0, 4);

        float spawnX = 0;
        float spawnY = 0;

        if (edge == 0)
        {
            spawnX = minX;
            spawnY = Random.Range(minY, maxY);
        }
        else if (edge == 1)
        {
            spawnX = maxX;
            spawnY = Random.Range(minY, maxY);
        }
        else if (edge == 2)
        {
            spawnX = Random.Range(minX, maxX);
            spawnY = maxY;
        }
        else
        {
            spawnX = Random.Range(minX, maxX);
            spawnY = minY;
        }

        Vector3 screenPos = new Vector3(
            spawnX * Screen.width,
            (1 - spawnY) * Screen.height,
            2f
        );

        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    void SpawnEffect(GameObject effect, Vector3 position)
    {
        GameObject instance = Instantiate(effect, position, Quaternion.identity);
        Destroy(instance, EFFECT_DURATION);
    }

    void ShowCelebrateEffect()
    {
        float depth = 2f;

        Vector3 left = Camera.main.ScreenToWorldPoint(
            new Vector3(-100, Screen.height / 2, depth));

        Vector3 right = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width + 100, Screen.height / 2, depth));

        Vector3 top = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width / 2, Screen.height + 100, depth));

        SpawnEffect(confettiEffect, left);
        SpawnEffect(confettiEffect, right);
        SpawnEffect(confettiEffect, top);

        SpawnRandomBalloons();
    }

    void SpawnRandomBalloons()
    {
        if (balloonPrefab == null) return;

        int balloonCount = Random.Range(3, 6);

        for (int i = 0; i < balloonCount; i++)
        {
            float randomX = Random.Range(-100f, Screen.width + 100f);
            float depth = 2f;

            Vector3 screenPos = new Vector3(randomX, -100f, depth);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            GameObject balloon = Instantiate(balloonPrefab, worldPos, Quaternion.identity);
            Destroy(balloon, EFFECT_DURATION);
        }
    }
}