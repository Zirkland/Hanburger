using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private bool isShaking = false;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // 调用此方法开始晃动：duration 为持续时间（秒），magnitude 为晃动强度（单位：米）
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }

    System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }
}