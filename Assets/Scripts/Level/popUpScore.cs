using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpScore : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI popUpText;
    public AudioSource audioSource;
    public AudioClip scoreSound;



    private void Awake()
    {
        popUpText = GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();

    }

    private void Start()
    {
        audioSource.PlayOneShot(scoreSound);
        StartCoroutine(textShowing(popUpText));
    }

    public void SettingText(int score)
    {
        popUpText.SetText("+" + score.ToString());
    }

    IEnumerator textShowing(TextMeshProUGUI text)
    {
        yield return new WaitForSeconds(0.5f);
        float duration = 0f;
        while (duration < 0.5f)
        {
            duration += Time.deltaTime;
            text.color = Color.Lerp(text.color, new Color(text.color.r, text.color.g, text.color.b, 0), Time.unscaledDeltaTime);
            text.gameObject.transform.position += Vector3.up * Time.unscaledDeltaTime;
            yield return null;
        }
        text.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
