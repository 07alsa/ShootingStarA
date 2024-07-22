using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentGauge = maxGauge;
        SetBackgroundTransparent(chargeBarSliderLeft);
        SetBackgroundTransparent(chargeBarSliderRight);

        if (chargeBarSliderLeft != null && chargeBarSliderRight != null)
        {
            chargeBarSliderLeft.maxValue = maxGauge;
            chargeBarSliderRight.maxValue = maxGauge;
            chargeBarSliderLeft.value = currentGauge;
            chargeBarSliderRight.value = currentGauge;
        }

        goSpace.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.isDash)
        {
            autoChargeSkill();
        }

        if (currentGauge >= maxGauge && !isFlashing)
        {
            Debug.Log("ChargeBar Update: Starting charge effect");
            StartChargeEffect();
        }
        else if (currentGauge <= 0 && isFlashing)
        {
            Debug.Log("ChargeBar Update: Stopping charge effect");
            StopChargeEffect();
        }
    }

    private void SetBackgroundTransparent(Slider slider)
    {
        Image backgroundImage = slider.transform.Find("Background").GetComponent<Image>();
        if (backgroundImage != null)
        {
            Color transparentColor = backgroundImage.color;
            transparentColor.a = 0f;
            backgroundImage.color = transparentColor;
        }
    }

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

        float duration = 2.0f;
        float startValue = maxGauge;
        float endValue = 0;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentGauge = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            chargeBarSliderLeft.value = currentGauge;
            chargeBarSliderRight.value = currentGauge;
            yield return null;
        }

        currentGauge = endValue;
        chargeBarSliderLeft.value = currentGauge;
        chargeBarSliderRight.value = currentGauge;
    }

    public void ChargeSkill(float amount)
    {
        currentGauge += amount;
        if (currentGauge > maxGauge)
        {
            currentGauge = maxGauge;
        }
        chargeBarSliderLeft.value = currentGauge;
        chargeBarSliderRight.value = currentGauge;
    }

    public void autoChargeSkill()
    {
        currentGauge += autoChargeSpeed * Time.deltaTime;
        if (currentGauge > maxGauge)
        {
            currentGauge = maxGauge;
        }
        chargeBarSliderLeft.value = currentGauge;
        chargeBarSliderRight.value = currentGauge;
    }

    private void StartChargeEffect()
    {
        if (chargeEffectCoroutine == null)
        {
            Debug.Log("StartChargeEffect: Starting charge effect");
            chargeEffectCoroutine = StartCoroutine(ChargeEffectCoroutine());
            isFlashing = true;
            goSpace.gameObject.SetActive(true);
            audioSource.PlayOneShot(chargedSound);
        }
    }

    private void StopChargeEffect()
    {
        if (chargeEffectCoroutine != null)
        {
            Debug.Log("StopChargeEffect: Stopping charge effect");
            StopCoroutine(chargeEffectCoroutine);
            chargeEffectCoroutine = null;
        }

        Color targetColor;
        ColorUtility.TryParseHtmlString("#BF94E4", out targetColor);
        chargeBarImageLeft.color = targetColor;
        chargeBarImageRight.color = targetColor;
        isFlashing = false;
        goSpace.gameObject.SetActive(false);
    }

    private IEnumerator ChargeEffectCoroutine()
    {
        float timer = 0f;
        float duration = 2f;

        while (true)
        {
            timer += Time.deltaTime / duration;
            float hue = Mathf.Repeat(timer, 1f);
            Color newColor = Color.HSVToRGB(hue, 0.3f, 1f);
            chargeBarImageLeft.color = newColor;
            chargeBarImageRight.color = newColor;

            yield return null;
        }
    }

    public void ChargeToMax()
    {
        StopCoroutine(gaugeDecreasing());
        if (chargeEffectCoroutine != null)
        {
            StopCoroutine(chargeEffectCoroutine);
        }
        chargeEffectCoroutine = null;

        currentGauge = maxGauge;
        chargeBarSliderLeft.value = currentGauge;
        chargeBarSliderRight.value = currentGauge;

        StartChargeEffect();
    }
}
