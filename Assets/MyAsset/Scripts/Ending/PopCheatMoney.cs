using UnityEngine;
using TMPro;

public class PopCheatMoney : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField moneyInputField; // 숫자를 입력할 칸

    // 💡 창이 열릴 때마다 입력칸을 비워주고 커서를 깜빡이게 해주는 센스!
    private void OnEnable()
    {
        if (moneyInputField != null)
        {
            moneyInputField.text = "";
            moneyInputField.ActivateInputField(); // 마우스 클릭 없이 바로 타이핑 가능!
        }
    }

    // 💡 [확인(변경)] 버튼을 눌렀을 때
    public void OnClickConfirm()
    {
        if (PlayerManager.Instance == null || moneyInputField == null) return;

        string inputStr = moneyInputField.text;

        if (long.TryParse(inputStr, out long parsedMoney))
        {
            // 1. 음수 방지
            long finalMoney = System.Math.Max(0, parsedMoney);

            // 2. 돈 강제 세팅
            PlayerManager.Instance.money = finalMoney;

            // 3. UI 갱신 (0원을 더하는 방식으로 꼼수 업데이트)
            PlayerManager.Instance.AddMoney(0);

            // 4. 엔딩 즉시 체크! (1조원 입력하면 창 닫히자마자 바로 선 넘음 엔딩 뜹니다)
            if (EndingManager.Instance != null) EndingManager.Instance.CheckInstantEndings();

            Debug.Log($"🚀 [치트] 보유 자산을 {finalMoney:N0}원으로 변경했습니다!");

            // 5. 볼일 끝났으니 창 닫기
            ClosePopup();
        }
        else
        {
            Debug.LogWarning("❌ [치트 실패] 숫자가 아니거나 너무 큰 값입니다.");
        }
    }

    // ❌ [취소] 버튼을 눌렀을 때
    public void OnClickCancel()
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false);
    }
}