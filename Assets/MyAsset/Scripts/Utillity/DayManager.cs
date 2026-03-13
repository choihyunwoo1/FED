using UnityEngine;
using System;

// 💡 하루의 3가지 상태
public enum DayPhase { Morning, Trading, Evening }

// 💡 인스펙터에서 권한을 껐다 켰다 할 수 있는 마법의 체크박스 묶음!
[System.Serializable]
public class PhasePermissions
{
    public bool canOpenNetwork = true;    // 데이트(외출)
    public bool canOpenStockList = true;  // 주식 시장 (이것만 막으면 차트도 안 열림!)
    public bool canOpenPortfolio = true;  // 포트폴리오
    public bool canOpenBriefing = true;   // 뉴스
    public bool canOpenGovernance = true; // 공작(로비)
}

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Current Time")]
    public int currentDay = 1;
    public int currentHour = 8;    // 아침 8시 시작
    public int currentMinute = 0;
    public DayPhase currentPhase = DayPhase.Morning; // 시작은 무조건 '아침'

    [Header("Time Flow Settings")]
    public float timePerTick = 1f;
    private float timer = 0f;
    public bool isTimeFlowing = false; // 시간이 흐르고 있는지 여부

    [Header("Phase Permissions (체크박스 세팅!)")]
    public PhasePermissions morningPerms; // 아침 권한
    public PhasePermissions tradingPerms; // 매매 권한
    public PhasePermissions eveningPerms; // 오후/휴식 권한

    // 💡 HUD에게 "시간 바뀌었어! 글씨 업데이트해!" 라고 알려주는 방송국
    public event Action OnTimeChanged;

    private void Awake() { Instance = this; }

    private void Start()
    {
        // 게임 시작 시 아침 8시로 세팅
        SetTime(8, 0);
    }

    private void Update()
    {
        // 시간이 흐르지 않는 상태(아침, 오후)면 아무것도 안 함!
        if (!isTimeFlowing) return;

        timer += Time.deltaTime;
        if (timer >= timePerTick)
        {
            timer = 0f;
            currentMinute += 5;

            // 💡 장이 열려있을 때만 주가 변동!
            if (currentPhase == DayPhase.Trading && StockManager.Instance != null)
            {
                StockManager.Instance.UpdateAllStocks();
            }

            if (currentMinute >= 60)
            {
                currentHour++;
                currentMinute = 0;
            }

            // 💡 오후 6시 되면 알아서 장 마감!
            if (currentHour >= 18 && currentMinute >= 0)
            {
                EndTrading();
            }

            OnTimeChanged?.Invoke(); // HUD 업데이트 신호 발송
        }
    }

    // --- 🎮 페이즈 전환 함수 (버튼으로 연결할 녀석들) ---

    // [장 시작] 버튼을 누르면 호출!
    public void StartTrading()
    {
        currentPhase = DayPhase.Trading;
        isTimeFlowing = true;
        SetTime(9, 0); // 9시로 강제 세팅
        if (NewsManager.Instance != null) NewsManager.Instance.isMarketOpen = true; // 찌라시 시작
    }

    // 시간 다 되거나 강제로 [장 마감] 할 때 호출!
    public void EndTrading()
    {
        currentPhase = DayPhase.Evening;
        isTimeFlowing = false;
        if (NewsManager.Instance != null) NewsManager.Instance.isMarketOpen = false; // 찌라시 정지
        OnTimeChanged?.Invoke(); // 마감 글씨 띄우기 위해 HUD 호출
    }

    // [다음 날로 가기(취침)] 버튼을 누르면 호출!
    public void NextDay()
    {
        currentDay++;
        currentPhase = DayPhase.Morning;
        isTimeFlowing = false;
        SetTime(8, 0); // 다시 다음날 아침 8시로
    }

    private void SetTime(int h, int m)
    {
        currentHour = h;
        currentMinute = m;
        OnTimeChanged?.Invoke();
    }

    // 🛡️ 권한 체크 함수
    public bool CheckPermission(string popupName)
    {
        PhasePermissions currentPerms = currentPhase == DayPhase.Morning ? morningPerms :
                                        currentPhase == DayPhase.Trading ? tradingPerms : eveningPerms;

        switch (popupName)
        {
            case "PopNetwork": return currentPerms.canOpenNetwork;     // 외출 권한
            case "PopStockList": return currentPerms.canOpenStockList;   // 시장 열기 권한 (대문)
            case "PopPortfolio": return currentPerms.canOpenPortfolio;   // 내 가방
            case "PopBriefing": return currentPerms.canOpenBriefing;    // 뉴스
            case "PopGovernance": return currentPerms.canOpenGovernance;  // 로비

            // 💡 PopMarket(차트)은 어차피 StockList를 거쳐야 하므로 무조건 통과!
            case "PopMarket": return true;

            default: return true;
        }
    }
}