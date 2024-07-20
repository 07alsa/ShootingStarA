using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChargeBar : MonoBehaviour
{
    public static ChargeBar Instance { get; private set; }

    public Slider chargeBarSliderLeft; // SliderLeft UI 컴포넌트 참조
    public Image chargeBarImageLeft;   // SliderLeft 색상을 변경할 Image 컴포넌트 참조
    public Slider chargeBarSliderRight; // SliderRight UI 컴포넌트 참조
    public Image chargeBarImageRight;  // SliderRight 색상을 변경할 Image 컴포넌트 참조
    public int maxGauge = 100;     // 최대 게이지 값
    public float currentGauge;    // 현재 게이지 값 (float으로 변경)
    public float skillUsageRate = 1f; // 스킬 사용 시 게이지 소모 속도 (per second)
    private Coroutine chargeEffectCoroutine; // 차지 색상 변화 코루틴 참조 변수
    [SerializeField] float autoChargeSpeed;
    private bool isFlashing; // 반짝이는 중인지 여부

    public GameObject goSpace;
    public AudioSource audioSource; // 오디오 소스 컴포넌트 참조
    public AudioClip chargedSound; // 차지완료 됐을 때 재생할 오디오 클립

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentGauge = maxGauge; // 현재 게이지를 최대값으로 초기화
        // 슬라이더의 백그라운드 이미지를 투명하게 설정
        SetBackgroundTransparent(chargeBarSliderLeft);
        SetBackgroundTransparent(chargeBarSliderRight);

        if (chargeBarSliderLeft != null && chargeBarSliderRight != null)
        {
            chargeBarSliderLeft.maxValue = maxGauge; // 슬라이더의 최대 값을 설정
            chargeBarSliderRight.maxValue = maxGauge;
            chargeBarSliderLeft.value = currentGauge; // 초기 게이지 값 설정
            chargeBarSliderRight.value = currentGauge;
        }

        goSpace.gameObject.SetActive(false); // 텍스트 비활성화
    }

    private void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.isDash)
        {
            autoChargeSkill();
        }

        // 게이지가 맥스에 도달하고 반짝이는 중이지 않다면 반짝이기 시작
        if (currentGauge >= maxGauge && !isFlashing)
        {
            StartChargeEffect();
        }
        // 게이지가 0이 되면 반짝이는 효과 중지
        else if (currentGauge <= 0 && isFlashing)
        {
            StopChargeEffect();
        }
    }

    // 슬라이더의 백그라운드 이미지를 투명하게 설정하는 함수
    private void SetBackgroundTransparent(Slider slider)
    {
        Image backgroundImage = slider.transform.Find("Background").GetComponent<Image>();
        if (backgroundImage != null)
        {
            Color transparentColor = backgroundImage.color;
            transparentColor.a = 0f; // 알파 값을 0으로 설정하여 완전히 투명하게 만듦
            backgroundImage.color = transparentColor;
        }
    }

    // 발판을 부술 때 호출되는 함수
    public void IncreaseGauge()
    {
        if (currentGauge < maxGauge)
        {
            currentGauge++;
            chargeBarSliderLeft.value = currentGauge;
            chargeBarSliderRight.value = currentGauge;
        }
    }

    public void UseSkill()
    {
        StartCoroutine(gaugeDecreasing());
    }

    private IEnumerator gaugeDecreasing()
    {
        goSpace.SetActive(false);

        float duration = 2.0f; // 애니메이션 지속 시간
        float startValue = maxGauge; // 슬라이더의 초기 값
        float endValue = 0; // 슬라이더의 최종 값
        float elapsedTime = 0; // 경과 시간

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentGauge = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            chargeBarSliderLeft.value = currentGauge;
            chargeBarSliderRight.value = currentGauge;
            yield return null;
        }

        currentGauge = endValue;
        chargeBarSliderLeft.value = currentGauge; // 애니메이션이 끝난 후 최종 값을 설정
        chargeBarSliderRight.value = currentGauge;
    }

    // 스킬 게이지를 증가시키는 함수
    public void ChargeSkill(float amount)
    {
        currentGauge += amount;
        if (currentGauge > maxGauge)
        {
            currentGauge = maxGauge;
        }
        chargeBarSliderLeft.value = currentGauge; // 슬라이더 업데이트
        chargeBarSliderRight.value = currentGauge;
    }

    public void autoChargeSkill()
    {
        currentGauge += autoChargeSpeed * Time.deltaTime;
        if (currentGauge > maxGauge)
        {
            currentGauge = maxGauge;
        }
        chargeBarSliderLeft.value = currentGauge; // 슬라이더 업데이트
        chargeBarSliderRight.value = currentGauge;
    }

    // 차지 색상 효과 시작
    private void StartChargeEffect()
    {
        if (chargeEffectCoroutine == null)
        {
            chargeEffectCoroutine = StartCoroutine(ChargeEffectCoroutine());
            isFlashing = true; // 반짝이는 중임을 표시
            goSpace.gameObject.SetActive(true);// 텍스트 활성화
            audioSource.PlayOneShot(chargedSound);//차지완료 사운드 재생
        }
    }

    // 차지 색상 효과 중지
    private void StopChargeEffect()
    {
        if (chargeEffectCoroutine != null)
        {
            StopCoroutine(chargeEffectCoroutine);
            chargeEffectCoroutine = null;
        }

        // 원래 색상으로 복원
        Color targetColor;
        ColorUtility.TryParseHtmlString("#BF94E4", out targetColor);
        chargeBarImageLeft.color = targetColor;
        chargeBarImageRight.color = targetColor;
        isFlashing = false; // 반짝이는 중이 아님을 표시
        goSpace.gameObject.SetActive(false);// 텍스트 비활성화
    }

    // 차지 색상 효과 코루틴
    private IEnumerator ChargeEffectCoroutine()
    {
        float timer = 0f;
        float duration = 2f;

        while (true)
        {
            timer += Time.deltaTime / duration;
            float hue = Mathf.Repeat(timer, 1f);  // hue 값이 0에서 1 사이를 반복
            Color newColor = Color.HSVToRGB(hue, 0.3f, 1f);  // HSV 값을 RGB로 변환
            chargeBarImageLeft.color = newColor; // 왼쪽 이미지의 색상 변경
            chargeBarImageRight.color = newColor; // 오른쪽 이미지의 색상 변경

            yield return null;  // 다음 프레임까지 대기
        }
    }

    public void ChargeToMax()
    {
        currentGauge = maxGauge;
        chargeBarSliderLeft.value = currentGauge;
        chargeBarSliderRight.value = currentGauge;

        StartChargeEffect(); // 차지 바를 빛나도록 하는 메서드 호출
        Debug.Log("ChargeBar maxed out.");
    }

}


