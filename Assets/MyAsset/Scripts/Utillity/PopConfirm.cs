using UnityEngine;
using TMPro;
using System; // 💡 Action을 쓰기 위해 필수!

public class PopConfirm : MonoBehaviour
{
    public static PopConfirm Instance; // 💡 어디서든 부를 수 있게 싱글톤 처리

    [Header("UI 연결")]
    public TextMeshProUGUI titleText;   // 💡 [추가됨] 팝업 제목이 들어갈 자리 (옵션)
    public TextMeshProUGUI messageText;

    private Action onConfirmCallback;  // 💡 [핵심] "확인 누르면 이거 실행해!" 하고 건네받은 함수표

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // 시작할 땐 숨겨둠
    }

    // 💡 [기존 유지] 기존에 다른 곳에서 쓰던 2개짜리 함수 (에러 방지용)
    public void ShowPopup(string message, Action onConfirm)
    {
        gameObject.SetActive(true);
        if (titleText != null) titleText.text = "알림"; // 제목란이 있으면 기본 '알림'으로 채움
        messageText.text = message;
        onConfirmCallback = onConfirm;
    }

    // 💡 [새로 추가!] 방금 기부 버튼에서 불렀던 3개짜리(제목, 내용, 함수) 셋업 메서드!
    public void SetupMessage(string title, string message, Action onConfirm)
    {
        gameObject.SetActive(true);
        if (titleText != null) titleText.text = title; // 제목 설정!
        messageText.text = message;
        onConfirmCallback = onConfirm;
    }

    public void OnClickConfirm()
    {
        // 💡 확인을 누르면 저장해둔 행동을 실행하고 창을 닫음!
        if (onConfirmCallback != null) onConfirmCallback.Invoke();

        // (UIManager를 통해 열렸다면 깔끔하게 UIManager로 닫는 것도 좋습니다!)
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false); // 취소하면 그냥 닫음
    }
}