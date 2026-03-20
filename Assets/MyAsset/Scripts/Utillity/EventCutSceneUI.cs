using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EventCutSceneUI : MonoBehaviour
{
    public Image characterImage;          // 사진 들어갈 자리
    public TextMeshProUGUI dialogueText;  // 대사 들어갈 자리

    // 💡 매니저가 데이터를 던져주면 세팅하는 함수!
    public void SetupStory(Sprite img, string text)
    {
        if (img != null)
        {
            characterImage.sprite = img;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false); // 사진 없으면 숨김
        }

        dialogueText.text = text;
    }

    public void OnClickClose()
    {
        // 💡 만약 현재 팝업이 '엔딩 팝업' 상태라면 창을 닫는 게 아니라 씬을 이동시킵니다!
        if (EndingManager.Instance != null && EndingManager.Instance.isGameEnded)
        {
            // "CreditsScene" 이라는 이름의 다른 씬으로 넘어갑니다! 
            // (File -> Build Settings에 씬이 등록되어 있어야 함)
            SceneManager.LoadScene("CreditsScene");
        }
        else
        {
            // 엔딩이 아닐 땐 평소처럼 팝업만 닫기
            UIManager.Instance.CloseCurrentPanel();
        }
    }
}
