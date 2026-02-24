using UnityEngine;

public class BalloonFloat : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * 1.5f);
    }
}
