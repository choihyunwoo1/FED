using UnityEngine;
using UnityEngine.UI;

public class DonationButtonUI : MonoBehaviour
{
    public GameObject donateButtonObject; // 기부 버튼 자체를 껐다 켰다 할 용도

    // 💡 [수정됨] OnEnable 대신 Update를 써서 매 프레임 가방을 실시간 감시합니다!
    private void Update()
    {
        CheckBible();
    }

    private void CheckBible()
    {
        if (PlayerManager.Instance == null || donateButtonObject == null) return;

        // 가방에 성경책이 1개라도 있다면?
        if (PlayerManager.Instance.HasItem("성경책", 1))
        {
            // 꺼져있을 때만 켭니다 (매 프레임 SetActive 낭비 방지)
            if (!donateButtonObject.activeSelf) donateButtonObject.SetActive(true);
        }
        else
        {
            if (donateButtonObject.activeSelf) donateButtonObject.SetActive(false);
        }
    }

    public void OnClickDonate()
    {
        // 💡 [수정됨] UIManager의 클론 소환 대신, 하이라키에 있는 원본(Instance)을 직접 깨웁니다!
        if (PopConfirm.Instance != null)
        {
            PopConfirm.Instance.SetupMessage(
                "전 재산 기부",
                "당신의 모든 현금과 주식을 사회에 환원하시겠습니까?\n이 선택은 되돌릴 수 없습니다.",
                ExecuteDonation
            );
        }
        else
        {
            Debug.LogWarning("❌ 하이라키에 PopConfirm이 활성화되어 있지 않거나 없습니다!");
        }
    }

    private void ExecuteDonation()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DonateAllWealth();

            // 💡 컷씬도 엉뚱하게 복제되지 않도록 하이라키에 있는 원본을 직접 찾아서 씁니다.
            EventCutSceneUI cutscene = FindAnyObjectByType<EventCutSceneUI>(FindObjectsInactive.Include);
            if (cutscene != null)
            {
                cutscene.gameObject.SetActive(true); // 숨겨진 원본 켜기
                cutscene.SetupStory(null, "<color=yellow>[ 기부 완료 ]</color>\n\n모든 재산을 내려놓았습니다.\n마음이 한결 가벼워진 것 같습니다.");
            }
        }
    }
}