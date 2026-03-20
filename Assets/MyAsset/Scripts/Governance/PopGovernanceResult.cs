using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopGovernanceResult : MonoBehaviour
{
    [Header("UI 연결")]
    public Image resultImage;             // 띄워줄 사진 (결과 아이콘이나 NPC 얼굴 등)
    public TextMeshProUGUI resultText;    // 결과 보고서 텍스트가 들어갈 곳

    // 💡 매니저가 이 창을 열면서 데이터를 쓱 밀어넣어 줄 함수입니다.
    public void SetupResult(Sprite img, string message)
    {
        // 1. 사진 세팅 (사진이 없으면 이미지 칸을 아예 꺼버려서 글씨만 꽉 차게 만듭니다!)
        if (resultImage != null)
        {
            if (img != null)
            {
                resultImage.sprite = img;
                resultImage.gameObject.SetActive(true);
            }
            else
            {
                resultImage.gameObject.SetActive(false);
            }
        }

        // 2. 텍스트 세팅
        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    // ❌ 닫기 버튼 (확인 버튼)
    public void OnClickClose()
    {
        // 비서를 통해 깔끔하게 현재 패널을 닫습니다.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseCurrentPanel();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}