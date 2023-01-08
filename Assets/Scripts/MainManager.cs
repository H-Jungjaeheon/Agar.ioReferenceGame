using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("게임 시작 안내 문구 텍스트")]
    private TextMeshProUGUI guideText;

    [SerializeField]
    [Tooltip("페이드 애니메이션 오브젝트")]
    private GameObject fadeObj;

    [SerializeField]
    [Tooltip("종합 페이드 애니메이션에 쓰일 이미지")]
    private Image fadeImage;

    private Color fadeColor;

    private float multiplied;

    private bool isFadeOut;

    void Start()
    {
        StartSetting();
    }

    void StartSetting()
    {
        isFadeOut = true;
        multiplied = -0.75f;

        fadeColor = new Color(0,0,0,0);

        StartCoroutine(PressAnyKeyToStart());
        StartCoroutine(GuideFadeAnim());
    }

    IEnumerator PressAnyKeyToStart()
    {
        while (true)
        {
            if (Input.anyKeyDown)
            {
                StartCoroutine(GameStartFadeAnim());
                break;
            }

            yield return null;
        }
    }

    IEnumerator GuideFadeAnim()
    {
        while (true)
        {
            if ((isFadeOut && guideText.alpha < 0) || (isFadeOut == false && guideText.alpha > 1))
            {
                isFadeOut = !isFadeOut;
                multiplied *= -1;
            }

            guideText.alpha += Time.deltaTime * multiplied;
            
            yield return null;
        }
    }

    IEnumerator GameStartFadeAnim()
    {
        float nowAlpha = 0;

        fadeObj.SetActive(true);

        while (true)
        {
            fadeColor.a = nowAlpha;
            fadeImage.color = fadeColor;

            nowAlpha += Time.deltaTime;

            if (nowAlpha >= 1)
            {
                SceneManager.LoadScene(1);
            }

            yield return null;
        }
    }
}
