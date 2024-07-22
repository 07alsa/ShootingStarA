using UnityEngine;

public class ChargeItem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어가 대쉬 중인지 확인
            if (PlayerController.Instance.isDash)
            {
                // 대쉬 중에 아이템을 먹었을 때 게이지를 즉시 0으로 설정
                ChargeBar.Instance.StopAllCoroutines(); // 게이지 업데이트 코루틴 중지
                ChargeBar.Instance.currentGauge = 0;
                ChargeBar.Instance.chargeBarSliderLeft.value = 0;
                ChargeBar.Instance.chargeBarSliderRight.value = 0;
            }

            // ChargeToMax 메서드를 호출하여 차지 게이지를 가득 채움
            ChargeBar.Instance.ChargeToMax();

            // 아이템 오브젝트를 파괴
            Destroy(gameObject);
        }
    }
}
