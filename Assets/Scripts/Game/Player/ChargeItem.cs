using System.Collections;
using UnityEngine;

public class ChargeItem : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ChargeEffectCoroutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ChargeItem: Player entered trigger");

            if (PlayerController.Instance.isDash)
            {
                ChargeBar.Instance.StopAllCoroutines();
                ChargeBar.Instance.currentGauge = 0;
                ChargeBar.Instance.chargeBarSliderLeft.value = 0;
                ChargeBar.Instance.chargeBarSliderRight.value = 0;
                Debug.Log("ChargeItem: Player is dashing, gauge set to 0");
            }

            ChargeBar.Instance.ChargeToMax();
            Debug.Log("ChargeItem: ChargeToMax called");
            Destroy(gameObject);
        }
    }

    private IEnumerator ChargeEffectCoroutine()
    {
        float timer = 0f;
        float duration = 2f;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        while (true)
        {
            timer += Time.deltaTime / duration;
            float hue = Mathf.Repeat(timer, 1f);
            Color newColor = Color.HSVToRGB(hue, 0.3f, 1f);
            spriteRenderer.color = newColor;

            yield return null;
        }
    }
}
