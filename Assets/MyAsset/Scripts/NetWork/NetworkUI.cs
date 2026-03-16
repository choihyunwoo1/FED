using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    // 💡 닫기 버튼용 (기존 PopupController에 있던 것과 동일)
    public void OnClickCloseButton()
    {
        UIManager.Instance.CloseCurrentPanel();
    }

    // 💡 NPC의 얼굴(버튼)을 클릭했을 때 실행될 함수!
    // 인스펙터에서 버튼마다 0, 1, 2 숫자를 다르게 넣어줄 겁니다.
    public void OnClickNPCButton(int npcIndex)
    {
        // 1. 매니저에게 "오늘 만날 사람은 이 번호다!" 라고 알려줌
        if (DateManager.Instance != null)
        {
            DateManager.Instance.currentNpcIndex = npcIndex;
        }

        // 2. 비서에게 데이트 창(미연시 화면)을 열라고 지시함!
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel("PopDate");
        }
    }
}