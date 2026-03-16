using System.Collections.Generic;
using UnityEngine;

// 💡 1. 발동 조건을 선택하는 스위치 추가!
public enum TriggerCondition { ExactTime, PhaseEntry }

[System.Serializable]
public class ScheduledEvent
{
    public string memo;

    [Header("⏰ 언제 터뜨릴까? (조건 선택)")]
    public int targetDay;
    public TriggerCondition triggerCondition; // 시간으로 할래? 페이즈로 할래?

    [Header("▶ [Exact Time] 선택 시 (시간 흐를 때)")]
    public int targetHour;
    public int targetMinute;

    [Header("▶ [Phase Entry] 선택 시 (아침/오후 시작 때)")]
    public DayPhase targetPhase;

    [Header("📰 1. 뉴스 이벤트")]
    public bool triggerNews = false;
    public int newsIndexToTrigger;

    [Header("🎬 2. 컷씬 / 스토리 팝업")]
    public bool triggerCutscene = false;
    public string cutscenePanelName = "PopEventCutScene";

    // 💡 인스펙터에서 직접 사진과 대사를 넣을 수 있는 칸!
    public Sprite storyImage;
    [TextArea(3, 5)]
    public string storyText;

    [Header("🔒 3. 권한 강제 변경")]
    public bool overridePermission = false;
    public DayPhase phaseToOverride;
    public PhasePermissions newPermissions;

    [Header("🔥 4. 상장폐지 이벤트")]
    public bool triggerDelist = false;   // 상장폐지를 터뜨릴 것인가?
    public string delistStockName;       // 상장폐지 시킬 주식 이름 (예: "도지코인")

    [HideInInspector]
    public bool hasTriggered = false;
}

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    [Header("Calendar / Schedule (스토리 달력)")]
    public List<ScheduledEvent> scheduleList = new List<ScheduledEvent>();

    private void Awake() { Instance = this; }

    private void Start()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnTimeChanged += CheckSchedule;
        }
    }

    private void CheckSchedule()
    {
        if (DayManager.Instance == null) return;

        int currentDay = DayManager.Instance.currentDay;
        int currentHour = DayManager.Instance.currentHour;
        int currentMinute = DayManager.Instance.currentMinute;
        DayPhase currentPhase = DayManager.Instance.currentPhase;

        foreach (ScheduledEvent evt in scheduleList)
        {
            if (evt.hasTriggered || evt.targetDay != currentDay) continue;

            // 💡 1. '정확한 시간' 조건일 때
            if (evt.triggerCondition == TriggerCondition.ExactTime)
            {
                if (evt.targetHour == currentHour && evt.targetMinute == currentMinute)
                {
                    ExecuteEvent(evt);
                }
            }
            // 💡 2. '특정 페이즈 진입' 조건일 때 (아침이 되자마자! 오후가 되자마자!)
            else if (evt.triggerCondition == TriggerCondition.PhaseEntry)
            {
                if (evt.targetPhase == currentPhase)
                {
                    ExecuteEvent(evt);
                }
            }
        }
    }

    private void ExecuteEvent(ScheduledEvent evt)
    {
        evt.hasTriggered = true;
        Debug.Log($"🗓️ [캘린더 이벤트 발동] Day {evt.targetDay} - {evt.memo}");

        // 1. 뉴스
        if (evt.triggerNews && NewsManager.Instance != null)
        {
            NewsManager.Instance.TriggerNews(evt.newsIndexToTrigger);
        }

        // 2. 컷씬 강제로 열기
        if (evt.triggerCutscene && UIManager.Instance != null)
        {
            // 비서에게 창을 열라고 지시
            UIManager.Instance.OpenPanel(evt.cutscenePanelName);

            // 💡 창이 열렸으니, 방금 열린 그 창(StoryUI)을 찾아서 사진과 대사를 꽂아넣어 줌!
            EventCutSceneUI popup = FindAnyObjectByType<EventCutSceneUI>();
            if (popup != null)
            {
                popup.SetupStory(evt.storyImage, evt.storyText);
            }
        }

        // 3. 권한 덮어쓰기
        if (evt.overridePermission && DayManager.Instance != null)
        {
            if (evt.phaseToOverride == DayPhase.Morning)
                DayManager.Instance.morningPerms = evt.newPermissions;
            else if (evt.phaseToOverride == DayPhase.Trading)
                DayManager.Instance.tradingPerms = evt.newPermissions;
            else if (evt.phaseToOverride == DayPhase.Evening)
                DayManager.Instance.eveningPerms = evt.newPermissions;

            Debug.Log($"🔒 [{evt.phaseToOverride}] 시간대의 권한이 시나리오에 의해 강제 변경되었습니다!");
        }

        // 🔥 4. 상장폐지 강제 집행!
        if (evt.triggerDelist && StockManager.Instance != null)
        {
            // StockManager에게 해당 주식을 날려버리라고 명령합니다.
            StockManager.Instance.DelistStock(evt.delistStockName);
        }
    }
}