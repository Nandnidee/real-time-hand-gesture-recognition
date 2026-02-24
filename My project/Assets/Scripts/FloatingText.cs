using UnityEngine;

public class FloatingText : MonoBehaviour
{
    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime;
        transform.localScale += Vector3.one * Time.deltaTime * 0.5f;
    }
}