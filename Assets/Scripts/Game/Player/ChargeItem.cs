using UnityEngine;

public class ChargeItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ChargeBar.Instance.ChargeToMax();
            Destroy(gameObject);
        }
    }
}
