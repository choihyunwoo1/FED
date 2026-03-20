using UnityEngine;
using UnityEngine.UI;

public class DonationButtonUI : MonoBehaviour
{
    public GameObject donateButtonObject; // 기부 버튼 자체를 껐다 켰다 할 용도

    private void OnEnable()
    {
        CheckBible();
    }

    // 💡 창이 켜질 때마다 가방에 '성경책'이 있는지 검사합니다.
    private void CheckBible()
    {
        if (PlayerManager.Instance == null || donateButtonObject == null) return;

        // 가방에 성경책이 1개라도 있다면? 버튼을 짠! 하고 보여줍니다.
        if (PlayerManager.Instance.HasItem("성경책", 1))
        {
            donateButtonObject.SetActive(true);
        }
        else
        {
            // 없으면 평범한 삶을 살도록 버튼을 숨깁니다.
            donateButtonObject.SetActive(false);
        }
    }

    // 💡 [기부하기] 버튼을 눌렀을 때 실행될 함수
    public void OnClickDonate()
    {
        if (UIManager.Instance != null)
        {
            // 전 재산을 날리는 거니 실수로 누르지 않게 꼭 확인 창(PopConfirm)을 띄워줍니다!
            UIManager.Instance.OpenPanel("PopConfirm");

            PopConfirm popup = FindAnyObjectByType<PopConfirm>();
            if (popup != null)
            {
                // PopConfirm 창에 제목, 내용, 그리고 [YES]를 눌렀을 때 실행할 행동(Action)을 넘겨줍니다.
                popup.SetupMessage(
                    "전 재산 기부",
                    "당신의 모든 현금과 주식을 사회에 환원하시겠습니까?\n이 선택은 되돌릴 수 없습니다.",
                    ExecuteDonation // 👈 YES 누르면 이 함수 실행!
                );
            }
        }
    }

    // 💡 유저가 PopConfirm 창에서 [확인(YES)]을 최종적으로 눌렀을 때만 실행되는 진짜 기부 함수!
    private void ExecuteDonation()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DonateAllWealth();

            // 기부 완료 메시지를 띄워줍니다 (컷씬 재활용)
            UIManager.Instance.OpenPanel("PopEventCutScene");
            EventCutSceneUI cutscene = FindAnyObjectByType<EventCutSceneUI>();
            if (cutscene != null)
            {
                cutscene.SetupStory(null, "<color=yellow>[ 기부 완료 ]</color>\n\n모든 재산을 내려놓았습니다.\n마음이 한결 가벼워진 것 같습니다.");
            }
        }
    }
}