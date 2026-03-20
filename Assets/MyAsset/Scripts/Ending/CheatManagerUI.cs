using UnityEngine;
using TMPro; // 💡 InputField를 제어하기 위해 필수!

public class CheatManagerUI : MonoBehaviour
{
    [Header("UI 연결 (TextMeshPro 전용)")]
    // 💡 유니티 에디터에서 숫자를 입력할 그 칸입니다!
    public TMP_InputField moneyInputField;

    // 💡 [돈 세팅] 버튼을 눌렀을 때 실행될 함수
    public void OnClickSetMoney()
    {
        if (PlayerManager.Instance == null || moneyInputField == null) return;

        // 1. 입력칸의 글씨(string)를 가져옵니다.
        string inputStr = moneyInputField.text;

        // 2. 글씨가 숫자인지, 너무 큰 숫자는 아닌지 검사하며 long 타입으로 변환을 시도합니다.
        // (TryParse는 변환에 성공하면 true를 반환하고, 아니면 false를 반환하는 안전한 방식입니다.)
        if (long.TryParse(inputStr, out long parsedMoney))
        {
            // 3. 변환에 성공했다면, 음수가 되지 않도록 최소 0원으로 제한(Clamp)합니다.
            long finalMoney = System.Math.Max(0, parsedMoney);

            // 4. 플레이어의 전 재산을 입력받은 숫자로 즉시 덮어씌웁니다! (치트!)
            PlayerManager.Instance.money = finalMoney;

            // 5. [중요] HUD 화면의 돈 글씨도 즉시 새로고침합니다!
            PlayerManager.Instance.AddMoney(0);

            // 6. [디테일] 혹시 돈을 세팅하자마자 즉시 터져야 하는 엔딩이 있는지 체크합니다.
            // (예: 1조원을 입력하면 바로 '선을 넘었다' 엔딩이 떠야 하니까요!)
            if (EndingManager.Instance != null) EndingManager.Instance.CheckInstantEndings();

            Debug.Log($"🚀 [치트 발동] 보유 자산을 {finalMoney:N0}원으로 강제 세팅했습니다!");
        }
        else
        {
            // 숫자가 아니거나 형식이 잘못됐을 때 경고!
            Debug.LogWarning("❌ [치트 실패] 숫자 형식이 올바르지 않거나 너무 큰 숫자입니다.");
        }
    }
}