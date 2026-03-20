// PopGovernanceUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopGovernanceUI : MonoBehaviour
{
    [Header("상단 바 UI")]
    public TextMeshProUGUI playerInfluenceText; // 통합 영향력 표시 칸

    [Header("진영 버튼 및 느낌표 (!)")]
    // 올려주신 1, 2, 3 버튼 오브젝트들
    public GameObject politicalFactionBtn;
    public GameObject politicalNotiTag;     // '!' 오브젝트
    public GameObject eliteFactionBtn;
    public GameObject eliteNotiTag;
    public GameObject celebrityFactionBtn;
    public GameObject celebrityNotiTag;

    [Header("오른쪽 상세 퀘스트 패널")]
    public GameObject detailPanelParent;    // 평소엔 꺼둡니다.
    public Image npcImage;
    public TextMeshProUGUI dialogText;
    public Button giveButton;

    [Header("상세 패널 - 오른쪽 4개 스탯")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI factionTypeText;
    public TextMeshProUGUI successRateText; // % 붙여서 표시
    public TextMeshProUGUI likabilityText;

    private FactionType currentlySelectedFaction;

    // 💡 창이 켜질 때마다 실시간 영향력과 느낌표 상태 업데이트
    private void OnEnable()
    {
        if (GovernanceManager.Instance == null) return;

        // 영향력 갱신
        if (playerInfluenceText != null)
            playerInfluenceText.text = $"영향력: {GovernanceManager.Instance.globalPlayerInfluence}";

        // 느낌표(!) 상태 갱신
        UpdateNotiTags();

        // 상세 패널은 닫아둡니다.
        if (detailPanelParent != null) detailPanelParent.SetActive(false);
    }

    private void UpdateNotiTags()
    {
        if (GovernanceManager.Instance == null) return;
        politicalNotiTag.SetActive(GovernanceManager.Instance.HasQuestInFaction(FactionType.Political));
        eliteNotiTag.SetActive(GovernanceManager.Instance.HasQuestInFaction(FactionType.Elite));
        celebrityNotiTag.SetActive(GovernanceManager.Instance.HasQuestInFaction(FactionType.Celebrity));
    }

    // 1️⃣ 에디터에서 1,2,3 진영 버튼 클릭 시 이벤트 연결 (OnClickFaction(int factionNum))
    public void OnClickFactionButton(int factionNum)
    {
        // 숫자 0,1,2 를 FactionType enum으로 변환
        currentlySelectedFaction = (FactionType)factionNum;

        // 해당 진영에 퀘스트가 있는지 확인
        if (!GovernanceManager.Instance.HasQuestInFaction(currentlySelectedFaction))
        {
            // 퀘스트가 없으면 상세 패널 끄고 리턴
            if (detailPanelParent != null) detailPanelParent.SetActive(false);
            Debug.Log("여기엔 아무도 없습니다.");
            return;
        }

        // 퀘스트가 있다면 상세 패널 켜고 정보 업데이트
        GovernanceManager.CurrentDayQuest dayQuest = GovernanceManager.Instance.GetDailyQuestInfo(currentlySelectedFaction);

        if (dayQuest == null || dayQuest.isCompleted) // 퀘스트가 없거나 이미 완료했으면 리턴
        {
            if (detailPanelParent != null) detailPanelParent.SetActive(false);
            return;
        }

        // 패널 켜기
        if (detailPanelParent != null) detailPanelParent.SetActive(true);

        // 데이터 채워넣기
        NpcData npc = dayQuest.npc;
        QuestData quest = dayQuest.quest;

        if (npcImage != null) npcImage.sprite = npc.npcImage;
        if (dialogText != null) dialogText.text = quest.questDescription; // 대화창 내용

        // Give 버튼 활성화 (이미 완료한 퀘스트면 비활성화 하는 등의 로직 추가 가능)
        if (giveButton != null)
        {
            giveButton.interactable = true;
            giveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give"; // 버튼 글씨 초기화
        }

        // 오른쪽 스탯 4개
        if (npcNameText != null) npcNameText.text = npc.npcName;
        if (factionTypeText != null) factionTypeText.text = GetFactionKoreanName(npc.faction); // 한글 이름 함수

        // 실시간 호감도 가져오기
        int likeness = GovernanceManager.Instance.GetNpcLikability(npc.npcName);
        if (likabilityText != null) likabilityText.text = likeness.ToString();

        // 💡 성공률 계산 로직 (기초 성공률 + 호감도 비례 보너스)
        // 예: 호감도 10당 성공률 1% 증가, 최대 95%
        int finalSuccessRate = Mathf.Clamp(quest.baseSuccessRate + (likeness / 10), 0, 95);
        if (successRateText != null) successRateText.text = $"성공률: {finalSuccessRate}%";
    }

    // 2️⃣ 에디터에서 [Give] 버튼 클릭 시 이벤트 연결 (OnClickGive())
    public void OnClickGiveButton()
    {
        if (GovernanceManager.Instance == null) return;

        GovernanceManager.CurrentDayQuest dayQuest = GovernanceManager.Instance.GetDailyQuestInfo(currentlySelectedFaction);
        if (dayQuest == null || dayQuest.isCompleted) return;

        QuestData quest = dayQuest.quest;
        NpcData npc = dayQuest.npc;

        // 💡 1. 요구 사항 확인 및 지불 (돈, 주식, 영향력 3가지 모두 완벽 지원!)
        if (quest.reqType == QuestReqType.Money)
        {
            if (PlayerManager.Instance.money >= quest.reqValue)
            {
                PlayerManager.Instance.SubtractMoney(quest.reqValue, true);
            }
            else { Debug.LogWarning("❌ 돈이 부족합니다!"); return; }
        }
        else if (quest.reqType == QuestReqType.SellStock)
        {
            // 내 가방에 그 주식이 있는지, 수량은 충분한지 검사!
            if (PlayerManager.Instance.GetStockAmount(quest.targetStockName) >= quest.reqValue)
            {
                // 조건 만족 시 강제 매도! (가계부에도 손익이 꼼꼼히 기록됩니다)
                PlayerManager.Instance.SellStock(quest.targetStockName, quest.reqValue);
                Debug.Log($"📉 퀘스트 조건으로 {quest.targetStockName} {quest.reqValue}주를 넘겼습니다.");
            }
            else { Debug.LogWarning($"❌ {quest.targetStockName} 주식 수량이 부족합니다!"); return; }
        }
        else if (quest.reqType == QuestReqType.UseInfluence)
        {
            if (GovernanceManager.Instance.globalPlayerInfluence >= quest.reqValue)
            {
                GovernanceManager.Instance.globalPlayerInfluence -= quest.reqValue;
            }
            else { Debug.LogWarning("❌ 통합 영향력이 부족합니다!"); return; }
        }

        // 💡 2. 운명의 주사위 굴리기 (성공 확률 계산)
        int likeness = GovernanceManager.Instance.GetNpcLikability(npc.npcName);
        int finalSuccessRate = Mathf.Clamp(quest.baseSuccessRate + (likeness / 10), 0, 95);

        int roll = Random.Range(0, 100);
        bool isSuccess = roll < finalSuccessRate; // 주사위 숫자가 확률보다 낮으면 성공!

        // 💡 3. 사건 수첩에 적어두기 (내일 아침에 터짐!)
        GovernanceManager.Instance.AddPendingEvent(npc, quest, isSuccess);

        // 💡 4. 즉시 보상: 호감도 & 영향력 펌핑 및 알력 다툼
        GovernanceManager.Instance.AddNpcLikability(npc.npcName, 10); // 퀘스트 들어주면 호감도 +10
        GovernanceManager.Instance.globalPlayerInfluence += 5;        // 전체 영향력 +5
        GovernanceManager.Instance.ApplyFactionPenalty(currentlySelectedFaction, quest.otherFactionLikenessPenalty); // 남의 진영 깎기

        // 💡 영향력 데이터가 올랐으니, 화면의 글씨도 즉시 새로고침 해줍니다!
        if (playerInfluenceText != null)
            playerInfluenceText.text = $"영향력: {GovernanceManager.Instance.globalPlayerInfluence}";

        // 💡 5. UI 마무으리 (중복 클릭 방지)
        dayQuest.isCompleted = true;
        if (giveButton != null)
        {
            giveButton.interactable = false;
            giveButton.GetComponentInChildren<TextMeshProUGUI>().text = "계약 완료";
        }

        // 화면 글씨 즉시 새로고침
        OnClickFactionButton((int)currentlySelectedFaction);

        Debug.Log($"🤝 [{npc.npcName}]과의 로비 계약 체결! 내일 아침을 기대하세요. (성공 여부: {isSuccess})");
    }

    // ❌ 닫기 버튼
    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    // 진영 영문 이름을 한글로 변환해주는 도구 함수
    private string GetFactionKoreanName(FactionType type)
    {
        switch (type)
        {
            case FactionType.Political: return "<color=red>정치계</color>";
            case FactionType.Elite: return "<color=green>엘리트계</color>";
            case FactionType.Celebrity: return "<color=blue>연예계</color>";
            default: return "알 수 없음";
        }
    }
}