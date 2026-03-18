using UnityEngine;
using TMPro;
using System; // 💡 Action을 쓰기 위해 필수!

public class PopConfirm : MonoBehaviour
{
    public static PopConfirm Instance; // 💡 어디서든 부를 수 있게 싱글톤 처리

    public TextMeshProUGUI messageText;
    private Action onConfirmCallback;  // 💡 [핵심] "확인 누르면 이거 실행해!" 하고 건네받은 함수표

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // 시작할 땐 숨겨둠
    }

    // 💡 팝업을 열 때 "메시지"와 "실행할 행동(Action)"을 같이 넘겨받습니다!
    public void ShowPopup(string message, Action onConfirm)
    {
        gameObject.SetActive(true);
        messageText.text = message;
        onConfirmCallback = onConfirm; // 받은 행동을 저장해둠
    }

    public void OnClickConfirm()
    {
        // 💡 확인을 누르면 저장해둔 행동을 실행하고 창을 닫음!
        if (onConfirmCallback != null) onConfirmCallback.Invoke();
        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false); // 취소하면 그냥 닫음
    }
}