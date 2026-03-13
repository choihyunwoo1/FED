using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
}
