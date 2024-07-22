using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlatformStaticHp : MonoBehaviour
{
    [SerializeField]
    private int hp;

    public PopUpScore scoreTextPfb;
    private PopUpScore scoreText;
    public DayNightCycle dayNightCycle; // DayNightCycle 스크립트 참조


    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        if (collisionInfo.collider.tag == "Player")
        {
            if (CanBreak())
            {
                if (!PlayerController.Instance.isDash)
                    PlayerController.Instance.Bounce(transform.position.y);
                GameManager.Instance.AddScore(CalculateScore());

                scoreText = Instantiate(scoreTextPfb, transform.position, Quaternion.identity);
                scoreText.SettingText(CalculateScore());
                gameObject.SetActive(false);

            }
            else
            {
                //죽으면 게임 멈춤
                GameManager.Instance.FailGame();

            }

        }
    }
    public int CalculateScore()
    {
        int basicPoint = 100;
        return basicPoint * (1 + 2 * hp);

    }
    private bool CanBreak()
    {
        if (PlayerController.Instance.ACCStep >= hp || PlayerController.Instance.isDash)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
}