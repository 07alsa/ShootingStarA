using System.Collections;
using JetBrains.Annotations;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    Rigidbody2D rigid;
    TrailRenderer trail;
    [SerializeField] SpriteRenderer sprite1;
    [SerializeField] SpriteRenderer sprite2;

    [SerializeField] TextMeshProUGUI velocityTextPfb;
    TextMeshProUGUI velocityText;

    [SerializeField] ParticleSystem particle;
    [SerializeField] ParticleSystem dashExplosionEffect; // 대쉬 종료 시 폭죽 효과
    ChargeBar chargeBar;
    [SerializeField] float HorizontalSpeed;
    [SerializeField] float dashSpeed;
    [SerializeField] float bounceForce;
    [SerializeField] float maxFallingSpeed;
    [SerializeField] int colorStep;
    Vector3 rotationSpeed; // 초당 90도 회전 (Z축 기준)
    [SerializeField] float colorRange;
    public bool isDash = false;
    public int ACCStep;
    Coroutine dashCoroutine;
    Coroutine bounceCoroutine = null;
    float saveAcc;
    bool isBouncing = false;
    WallMove wallmove;
    Canvas canvas;
    public AudioSource audioSource; // 오디오 소스 컴포넌트 참조
    public AudioClip dashSound; // 대쉬 떄 재생할 오디오 클립
    public AudioClip boomSound;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        trail = GetComponentInChildren<TrailRenderer>();
        wallmove = GetComponent<WallMove>();
        audioSource = GetComponent<AudioSource>();


        colorRange = maxFallingSpeed / colorStep;
        // slider = FindObjectOfType<Slider>();
        chargeBar = FindObjectOfType<ChargeBar>();

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        if (velocityTextPfb != null)
        {
            velocityText = Instantiate(velocityTextPfb, new Vector3(0, 4, 0), Quaternion.identity, canvas.transform);
            velocityText.transform.SetAsFirstSibling();
        }
    }

    // Update is called once per frame
    void Update()
    {
        dash();

        if (!isDash)
        {
            if (!isBouncing)
                changeColor();

            // 회전
            rotationSpeed = new Vector3(0, 0, (ACCStep + 1) * 180);
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (transform.rotation != Quaternion.Euler(0, 0, 180))
                // 무조건 뾰족한거 아래로 향하게
                transform.rotation = Quaternion.Euler(0, 0, 180);
        }

    }

    void FixedUpdate()
    {

        float horizontalInput = Input.GetAxis("Horizontal");
        // Debug.Log(horizontalInput);

        if (wallmove == null)
        {
            Debug.Log("WallMove Refer is null");
            return;
        }

        if (horizontalInput > 0 && !wallmove.CanMoveRight)
        {

            horizontalInput = 0;
        }

        else if (horizontalInput < 0 && !wallmove.CanMoveLeft)
        {

            horizontalInput = 0;
        }

        rigid.velocity = new Vector2(horizontalInput * HorizontalSpeed, rigid.velocity.y);

        if (rigid.velocity.y < -maxFallingSpeed)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, -maxFallingSpeed);
        }
    }

    void LateUpdate()
    {
        velocityText.transform.position = new Vector3(transform.position.x, velocityText.transform.position.y, velocityText.transform.position.z);
    }

    void changeColor()
    {
        ACCStep = (int)(Mathf.Abs(rigid.velocity.y) / colorRange);
        if (ACCStep == colorStep)
        {
            ACCStep = colorStep - 1;
        }

        velocityText.SetText((ACCStep + 1).ToString());

        Color targetColor;
        switch (ACCStep)
        {
            case 0:
                ColorUtility.TryParseHtmlString("#e0ffff", out targetColor);

                StartCoroutine(LerpColorChnage(sprite1.color, targetColor));
                StartCoroutine(LerpTrailChnage(trail.startColor, targetColor));
                break;

            case 1:
                ColorUtility.TryParseHtmlString("#48d1cc", out targetColor);

                StartCoroutine(LerpColorChnage(sprite1.color, targetColor));
                StartCoroutine(LerpTrailChnage(trail.startColor, targetColor));
                break;


            case 2:
                ColorUtility.TryParseHtmlString("#4169e1", out targetColor);

                StartCoroutine(LerpColorChnage(sprite1.color, targetColor));
                StartCoroutine(LerpTrailChnage(trail.startColor, targetColor));
                break;


            case 3:
                ColorUtility.TryParseHtmlString("#570498", out targetColor);

                StartCoroutine(LerpColorChnage(sprite1.color, targetColor));
                StartCoroutine(LerpTrailChnage(trail.startColor, targetColor));
                break;
        }
    }

    IEnumerator LerpColorChnage(Color nowColor, Color targetColor)
    {
        float duration = 0.1f;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            sprite1.color = Color.Lerp(nowColor, targetColor, timeElapsed / duration);
            sprite2.color = Color.Lerp(nowColor, targetColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        sprite1.color = targetColor;
        sprite2.color = targetColor;
    }

    IEnumerator LerpTrailChnage(Color nowColor, Color targetColor)
    {
        float duration = 0.1f;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            trail.startColor = Color.Lerp(nowColor, targetColor, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        trail.startColor = targetColor;
    }


    void dash()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.DownArrow)) && chargeBar.currentGauge == chargeBar.maxGauge)
        {
            chargeBar.UseSkill();

            if (!isDash)
            {
                isDash = true;

                rigid.velocity = Vector2.zero;

                if (dashCoroutine != null)
                {
                    StopCoroutine(dashCoroutine);
                    dashCoroutine = StartCoroutine(dashEffect());
                }
                else
                {
                    dashCoroutine = StartCoroutine(dashEffect());
                }

                velocityText.SetText("<#f98cde>Fever!");
                velocityText.transform.SetAsLastSibling();
            }

            StartCoroutine(dashing());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ChargeItem"))
        {
            if (isDash)
            {
                chargeBar.StopAllCoroutines(); // 게이지 업데이트 코루틴 중지
                chargeBar.currentGauge = 0;
                chargeBar.chargeBarSliderLeft.value = 0;
                chargeBar.chargeBarSliderRight.value = 0;
            }

            chargeBar.ChargeToMax();

            Destroy(other.gameObject);
        }
    }
    public IEnumerator dashing()
    {
        float duration = 1.5f;
        float currentTime = 0f;
        float blinkStart = 1f; // 깜빡거림이 시작되는 시간
        float blinkFrequency = 1f; // 초기 깜빡거림 주기

        // 대쉬 시작 시 사운드 재생
        if (audioSource != null && dashSound != null)
        {
            audioSource.clip = dashSound;
            audioSource.Play();
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            if (Mathf.Abs(rigid.velocity.y) < maxFallingSpeed)
                rigid.velocity = new Vector2(rigid.velocity.x, -maxFallingSpeed);

            // 깜빡거림 시작
            if (currentTime >= blinkStart)
            {
                float blinkTimer = Mathf.PingPong((currentTime - blinkStart) * blinkFrequency, 1f);
                Color newColor = new Color(1f, 1f, 1f, blinkTimer);
                sprite1.color = newColor;
                sprite2.color = newColor;
                trail.startColor = newColor;

                // 깜빡거림 속도 증가
                if (currentTime >= duration - 1.75f)
                {
                    blinkFrequency = 27f; // 깜빡거림 속도 증가
                }
                else
                {
                    blinkFrequency = 2f; // 기본 깜빡거림 속도
                }
            }

            yield return null;
        }

        // 대쉬 종료 시 사운드 정지
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (isDash)
        {
            isDash = false;
            StopCoroutine(dashCoroutine);
        }

        // 대쉬 종료 시 폭죽 효과 및 플랫폼 제거 실행
        TriggerDashExplosion();
        RemovePlatformsInRadius(5f); // 반경 5 단위로 설정, 필요에 따라 조정 가능

        // 대쉬 종료 시 원래 색상으로 복원
        sprite1.color = Color.white;
        sprite2.color = Color.white;
        trail.startColor = Color.white;
    }

    IEnumerator dashEffect()
    {
        float timer = 0f;
        float duration = 0.5f;

        while (true)
        {
            timer += Time.deltaTime / duration;
            float hue = Mathf.Repeat(timer, 1f);  // hue 값이 0에서 1 사이를 반복
            Color newColor = Color.HSVToRGB(hue, 0.3f, 1f);  // HSV 값을 RGB로 변환
            sprite1.color = newColor;
            sprite2.color = newColor;
            trail.startColor = newColor;

            yield return null;  // 다음 프레임까지 대기
        }
    }

    float blockY;
    public void SaveAcc()
    {
        saveAcc = ACCStep;
    }
    public void Bounce(float yPos)
    {
        SaveAcc();

        chargeBar.ChargeSkill(10f);

        if (rigid.position.y < yPos)
        {
            rigid.velocity = Vector2.zero;
            float target_vel = ((saveAcc - 1) < 0 ? 0 : (saveAcc - 1)) * colorRange;

            rigid.velocity = new Vector2(rigid.velocity.x, -target_vel);
        }
        else
        {
            rigid.velocity = Vector2.zero;

            // 속도 구간에 따른 바운스 값을 설정
            float bounceMultiplier;
            if (isDash)
            {
                bounceMultiplier = 0f; // 대쉬 중일 때 바운스 값을 많이 줄임
            }
            else
            {
                switch (ACCStep)
                {
                    case 0:
                        bounceMultiplier = 1f; // 기본 바운스 값
                        break;
                    case 1:
                        bounceMultiplier = 1.2f; // 1단계 속도에서 1.2배
                        break;
                    case 2:
                        bounceMultiplier = 1.3f; // 2단계 속도에서 1.3배
                        break;
                    case 3:
                        bounceMultiplier = 1.4f; // 3단계 속도에서 1.4배
                        break;
                    default:
                        bounceMultiplier = 1f; // 기본 바운스 값
                        break;
                }
            }

            rigid.AddForce(Vector2.up * bounceForce * bounceMultiplier, ForceMode2D.Impulse); // 속도에 따른 튀어오름

            if (bounceCoroutine != null)
            {
                StopCoroutine(bounceCoroutine);
                bounceCoroutine = StartCoroutine(checkBounceReverse());
            }
            else
            {
                bounceCoroutine = StartCoroutine(checkBounceReverse());
            }
        }
    }

    IEnumerator checkBounceReverse()
    {
        while (true)
        {
            if (!isBouncing)
            {
                isBouncing = true;
                velocityText.SetText("");
            }

            if (rigid.velocity.y < 0)
            {
                if (isBouncing)
                {
                    isBouncing = false;
                }
                float target_vel = ((saveAcc - 1) < 0 ? 0 : (saveAcc - 1)) * colorRange;

                rigid.velocity = new Vector2(rigid.velocity.x, -target_vel);
                yield break;
            }

            yield return null;
        }
    }

    void OnDestroy()
    {
        ParticleSystem p = Instantiate(particle, transform.position, quaternion.identity);
        var module = p.main;
        module.startColor = sprite1.color;
        Destroy(velocityText.gameObject);
    }

    void TriggerDashExplosion()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // 플레이어의 가장 아래쪽 위치 계산
            Vector3 playerBottomPosition = transform.position - new Vector3(0, spriteRenderer.bounds.extents.y, 0);

            // 플레이어의 가장 아래쪽 위치보다 더 아래로 조정
            float offset = 4.0f; // 플레이어 아래쪽에서 추가로 내려갈 거리
            Vector3 explosionPosition = playerBottomPosition - new Vector3(0, offset, 0);

            // 폭죽 효과 실행
            ParticleSystem explosion = Instantiate(dashExplosionEffect, explosionPosition, Quaternion.identity);
            explosion.Play();
        }
        else
        {
            // 폭죽 효과 실행 (기본 위치에서)
            Vector3 explosionPosition = transform.position - new Vector3(0, 1.0f, 0); // 기본 위치에서 추가로 내려갈 거리
            ParticleSystem explosion = Instantiate(dashExplosionEffect, explosionPosition, Quaternion.identity);
            explosion.Play();
        }
        // 폭발 시 사운드 재생
        if (audioSource != null && boomSound != null)
        {
            audioSource.clip = boomSound;
            audioSource.Play();
        }
    }
    void RemovePlatformsInRadius(float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Platform"))
            {
                Destroy(hitCollider.gameObject);
            }
        }
    }
}
