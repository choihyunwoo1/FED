// GovernanceManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // FindIndex 등 쓰기 위해

public class GovernanceManager : MonoBehaviour
{
    public static GovernanceManager Instance;

    [Header("데이터 도감 연결")]
    // 모든 NPC 정보가 들어있는 SO 리스트 (에디터에서 넣어주세요)
    public List<NpcData> allNpcDatabase = new List<NpcData>();

    [Header("스폰 설정")]
    [Range(0, 100)]
    public int dailyEventChance = 30; // 매일 거버넌스 이벤트가 뜰 확률

    // 런타임 데이터 (💾 세이브 필요)
    [Header("플레이어 상태 데이터")]
    public int globalPlayerInfluence = 0; // 플레이어 통합 영향력

    // 딕셔너리: <NPC이름, 호감도>. 딕셔너리는 유니티 인스펙터에 안 보여서 세이브 시 리스트로 변환 필요.
    public Dictionary<string, int> npcLikability = new Dictionary<string, int>();

    // 오늘 활성화된 퀘스트 정보 <진영, 오늘 배정된 Npc/퀘스트>
    // 런타임용 클래스를 따로 만듭니다.
    [System.Serializable]
    public class CurrentDayQuest
    {
        public NpcData npc;
        public QuestData quest;
        public bool isCompleted = false;
    }
    private Dictionary<FactionType, CurrentDayQuest> activeDailyQuests = new Dictionary<FactionType, CurrentDayQuest>();

    // (GovernanceManager.cs의 변수 선언부 어딘가에 추가)

    [System.Serializable]
    public class PendingEvent
    {
        public NpcData npc;
        public QuestData quest;
        public bool isSuccess; // 주사위 굴림 결과
    }
    // 💡 유저가 로비에 성공해서 내일 아침에 터질 대기열입니다.
    public List<PendingEvent> tomorrowEvents = new List<PendingEvent>();

    // ==========================================
    // 💡 [여기서부터 새로운 함수들 추가!]

    // 1. 호감도 조절 함수 (증가/감소)
    public void AddNpcLikability(string npcName, int amount)
    {
        if (npcLikability.ContainsKey(npcName))
            npcLikability[npcName] = Mathf.Clamp(npcLikability[npcName] + amount, 0, 100);
        else
            npcLikability.Add(npcName, Mathf.Clamp(amount, 0, 100));
    }

    // 2. 다른 진영 페널티 (알력 다툼)
    public void ApplyFactionPenalty(FactionType supportedFaction, int penaltyAmount)
    {
        // 내가 도와준 진영 빼고 나머지 진영 NPC들의 호감도를 깎습니다!
        foreach (var kvp in npcLikability.ToList()) // ToList()로 복사본을 만들어 에러 방지
        {
            NpcData targetNpc = allNpcDatabase.Find(x => x.npcName == kvp.Key);
            if (targetNpc != null && targetNpc.faction != supportedFaction)
            {
                npcLikability[kvp.Key] = Mathf.Clamp(kvp.Value - penaltyAmount, 0, 100);
            }
        }
        Debug.Log($"⚖️ [{supportedFaction}] 진영을 도와주어 다른 진영의 호감도가 {penaltyAmount}만큼 감소했습니다.");
    }

    // 3. 사건 수첩에 등록!
    public void AddPendingEvent(NpcData npc, QuestData quest, bool isSuccess)
    {
        tomorrowEvents.Add(new PendingEvent { npc = npc, quest = quest, isSuccess = isSuccess });
    }

    private void Awake()
    {
        Instance = this;
        // 딕셔너리 초기화 (게임 시작 시 데이터베이스의 모든 NPC 호감도를 0으로)
        foreach (NpcData npc in allNpcDatabase)
        {
            if (!npcLikability.ContainsKey(npc.npcName))
                npcLikability.Add(npc.npcName, 0);
        }
    }

    // 🎮 매일 아침(DayManager.NextDay에서 호출) 실행될 로직
    public void DetermineDailyEvents()
    {
        activeDailyQuests.Clear(); // 어제 퀘스트 싹 정리

        // 1. 오늘 이벤트가 발생할지 주사위 굴림
        if (Random.Range(0, 100) > dailyEventChance) return; // 실패 시 오늘은 이벤트 없음

        // 2. 발생했다면, 3대 진영 중 하나를 랜덤 선택 (느낌표 띄울 곳)
        FactionType[] allFactions = { FactionType.Political, FactionType.Elite, FactionType.Celebrity };
        FactionType selectedFaction = allFactions[Random.Range(0, allFactions.Length)];

        // 3. 해당 진영 소속 NPC 중 한 명을 랜덤 선택
        List<NpcData> factionNpcs = allNpcDatabase.FindAll(npc => npc.faction == selectedFaction);
        if (factionNpcs.Count == 0) return; // 해당 진영에 등록된 NPC가 없으면 리턴

        NpcData selectedNpc = factionNpcs[Random.Range(0, factionNpcs.Count)];

        // 4. 그 NPC가 가진 퀘스트 목록 중 하나를 랜덤 선택
        if (selectedNpc.possibleQuests.Count == 0) return; // NPC가 퀘스트가 없으면 리턴
        QuestData selectedQuest = selectedNpc.possibleQuests[Random.Range(0, selectedNpc.possibleQuests.Count)];

        // 5. 오늘 퀘스트 장부에 등록
        activeDailyQuests.Add(selectedFaction, new CurrentDayQuest { npc = selectedNpc, quest = selectedQuest });

        Debug.Log($"🏛️ 거버넌스 이벤트 스폰: {selectedFaction} 진영의 [{selectedNpc.npcName}]이 [{selectedQuest.name}] 퀘스트를 가지고 대기 중입니다.");
    }

    // 🔍 특정 진영에 오늘 퀘스트가 있는지 확인하는 함수 (UI에서 '!' 띄울 때 사용)
    public bool HasQuestInFaction(FactionType faction)
    {
        return activeDailyQuests.ContainsKey(faction);
    }

    // 🔍 특정 진영의 오늘 퀘스트 정보를 가져오는 함수 (UI에 내용 표시할 때 사용)
    public CurrentDayQuest GetDailyQuestInfo(FactionType faction)
    {
        if (activeDailyQuests.ContainsKey(faction))
            return activeDailyQuests[faction];
        return null;
    }

    // 🔍 특정 NPC의 실시간 호감도 가져오기
    public int GetNpcLikability(string npcName)
    {
        if (npcLikability.ContainsKey(npcName)) return npcLikability[npcName];
        return 0;
    }

    // 💡 DayManager.NextDay()에서 아침이 밝았을 때 호출할 함수!
    public void ProcessPendingEvents()
    {
        if (tomorrowEvents.Count == 0) return; // 예약된 사건이 없으면 패스

        // 💡 팝업창에 띄울 '결과 보고서' 텍스트를 모아둘 빈 바구니입니다.
        string resultMessage = "<size=120%><b>[로비 결과 보고서]</b></size>\n\n";

        foreach (PendingEvent pEvent in tomorrowEvents)
        {
            if (pEvent.isSuccess)
            {
                resultMessage += $"<color=blue><b>[계약 성공]</b></color> {pEvent.npc.npcName} 측에서 약속을 이행했습니다.\n";

                // 성공했을 때 보상 지급!
                switch (pEvent.quest.rewardType)
                {
                    case QuestRewardType.Money:
                        PlayerManager.Instance.AddMoney(pEvent.quest.rewardValue, true);
                        resultMessage += $"▶ 보상: 지원금 {pEvent.quest.rewardValue:N0}원 입금\n\n";
                        break;

                    case QuestRewardType.StockBoost:
                        if (StockManager.Instance != null)
                        {
                            foreach (var kvp in PlayerManager.Instance.portfolio)
                            {
                                Stock s = StockManager.Instance.stockList.Find(x => x.stockName == kvp.Key);
                                // 💡 rewardValue를 퍼센트(%)로 사용합니다! (예: 10 입력 시 10% 상승)
                                if (s != null) s.currentPrice += Mathf.RoundToInt(s.currentPrice * (pEvent.quest.rewardValue / 100f));
                            }
                        }
                        resultMessage += $"▶ 보상: 보유 중인 모든 주식 {pEvent.quest.rewardValue}% 상승!\n\n";
                        break;

                        // (다른 보상들도 이곳에 추가 가능!)
                }
            }
            else
            {
                // 실패했을 때
                resultMessage += $"<color=red><b>[계약 실패]</b></color> {pEvent.npc.npcName}의 로비가 실패로 돌아갔습니다.\n▶ 결과: 투자금 손실\n\n";
            }
        }

        // 처리가 끝났으니 수첩은 백지로 초기화!
        tomorrowEvents.Clear();

        // ==================================================
        // 💡 [여기서 팝업창 호출!] 모아둔 결과 보고서를 화면에 띄웁니다!

        if (UIManager.Instance != null)
        {
            // 1. UIManager에게 새 팝업을 열라고 지시
            UIManager.Instance.OpenPanel("PopGovernanceResult");

            // 2. 켜진 팝업 스크립트를 찾아서 데이터 전달
            PopGovernanceResult popup = FindAnyObjectByType<PopGovernanceResult>();
            if (popup != null)
            {
                // 💡 사진은 일단 비워두고(null), 우리가 정성껏 쓴 결과 보고서를 넘겨줍니다!
                // (나중에 성공/실패 아이콘이나 NPC 얼굴을 넣고 싶으면 null 자리에 이미지를 넣으시면 됩니다)
                popup.SetupResult(null, resultMessage);
            }
        }
    }
}