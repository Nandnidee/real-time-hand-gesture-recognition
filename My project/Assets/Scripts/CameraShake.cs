using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;
    float shakeDuration = 0f;
    float shakeAmount = 0.15f;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void TriggerShake(float duration)
    {
        shakeDuration = duration;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }
}