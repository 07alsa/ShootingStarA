using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    public Camera mainCamera; // 메인 카메라
    public Image darkOverlay; // 어두운 화면을 덮는 이미지
    public float spotlightDuration = 10f; // 플레이어 주변을 밝게 하는 시간

    private Color[] colors = new Color[]
    {
        new Color(190f / 255f, 213f / 255f, 231f / 255f), // 아침
        new Color(204f / 255f, 195f / 255f, 164f / 255f), // 낮
        new Color(243f / 255f, 138f / 255f, 110f / 255f), // 석양
        new Color(201f / 255f, 109f / 255f, 127f / 255f), // 밤1
        new Color(125f / 255f, 94f / 255f, 128f / 255f), // 밤2
        new Color(87f / 255f, 68f / 255f, 111f / 255f), // 밤3
        new Color(19f / 255f, 26f / 255f, 98f / 255f)  // 밤4
    };

    private float[] transitionDurations = new float[]
    {
        10f, // 아침에서 낮으로
        10f, // 낮에서 석양으로
        5f,  // 석양에서 첫 번째 밤 색상으로
        5f,  // 첫 번째 밤 색상에서 두 번째 밤 색상으로
        5f,  // 두 번째 밤 색상에서 세 번째 밤 색상으로
        5f,  // 세 번째 밤 색상에서 마지막 밤 색상으로
        15f  // 마지막 밤 색상에서 아침으로
    };

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 메인 카메라를 자동으로 할당
        }

        if (darkOverlay != null)
        {
            darkOverlay.color = new Color(0f, 0f, 0f, 0f); // 시작할 때 어두운 오버레이를 투명하게 설정
        }

        StartCoroutine(CycleColors());
    }

    private IEnumerator CycleColors()
    {
        int colorIndex = 0;

        while (true)
        {
            // 색상 전환
            yield return StartCoroutine(FadeColor(colors[colorIndex], colors[(colorIndex + 1) % colors.Length], transitionDurations[colorIndex]));

            // 마지막 밤 색상에서 플레이어 주변을 밝히는 효과 적용
            if (colorIndex == colors.Length - 2) // 마지막 밤 색상 바로 전에
            {
                StartCoroutine(FadeToDark(true, 1f)); // 어둡게 전환
                yield return new WaitForSeconds(spotlightDuration);
                StartCoroutine(FadeToDark(false, 1f)); // 밝게 전환
            }

            // 색상 인덱스 업데이트
            colorIndex = (colorIndex + 1) % colors.Length;
        }
    }

    private IEnumerator FadeColor(Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            mainCamera.backgroundColor = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        mainCamera.backgroundColor = endColor; // 최종 색상 설정
    }

    private IEnumerator FadeToDark(bool toDark, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = darkOverlay.color;
        Color endColor = toDark ? new Color(0f, 0f, 0f, 0.9f) : new Color(0f, 0f, 0f, 0f); // 어둡게 또는 투명하게

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            darkOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        darkOverlay.color = endColor; // 최종 색상 설정
    }
}