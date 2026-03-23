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

        // 💡 1일차 아침 게임 시작 직후에도 거버넌스 주사위를 한 번 굴려줍니다!
        if (GovernanceManager.Instance != null)
        {
            GovernanceManager.Instance.DetermineDailyEvents();
        }
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
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        currentPhase = DayPhase.Trading;
        isTimeFlowing = true;
        SetTime(9, 0); // 9시로 강제 세팅
        if (NewsManager.Instance != null) NewsManager.Instance.isMarketOpen = true; // 찌라시 시작
    }

    // 시간 다 되거나 강제로 [장 마감] 할 때 호출!
    public void EndTrading()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        foreach (Stock stock in StockManager.Instance.stockList)
        {
            // 💡 [수정] 장이 마감되었으니, 지금 가격을 '어제 종가'로 확정 지어줍니다.
            // (고가/저가는 저녁에 유저가 확인할 수 있게 초기화하지 않고 내버려 둡니다!)
            stock.previousPrice = stock.currentPrice;
        }

        currentPhase = DayPhase.Evening;
        isTimeFlowing = false;
        if (NewsManager.Instance != null) NewsManager.Instance.isMarketOpen = false; // 찌라시 정지
        OnTimeChanged?.Invoke(); // 마감 글씨 띄우기 위해 HUD 호출
    }

    // [다음 날로 가기(취침)] 버튼을 누르면 호출!
    public void NextDay()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        // 💡 [추가] 오늘이 설정해둔 마지막 날(예: 300일)이라면?
        if (currentDay >= EndingManager.Instance.finalDay)
        {
            EndingManager.Instance.CheckLastDayEndings();
            return; // 다음 날로 안 넘어가고 게임 끝!
        }

        // 💡새로운 퀘스트를 스폰하기 '직전'에, 어제 예약해둔 결과를 먼저 빵! 터트립니다.
        GovernanceManager.Instance.ProcessPendingEvents();
        // 오늘 새로운 퀘스트 스폰!
        GovernanceManager.Instance.DetermineDailyEvents();

        // 💡 [추가] 아침에 눈을 떴을 때, 새로운 하루의 고가/저가 기록장을 리셋합니다!
        foreach (Stock stock in StockManager.Instance.stockList)
        {
            stock.todayHigh = stock.currentPrice;
            stock.todayLow = stock.currentPrice;
        }

        currentDay++;

        // 💡 [추가] 수금일 전날(예: 7일차, 14일차)에 미리 경고해주기!
        if (currentDay % 7 == 0 && PlayerManager.Instance.bankDebt > 0)
        {
            Debug.LogWarning("🚨 [은행 알림] 내일은 대출 이자 납부일입니다! 현금을 넉넉히 준비하세요!");
            // TODO: 나중에 화면에 빨간 글씨로 팝업 띄워주면 더 좋습니다!
        }

        // 💡 8일, 15일, 22일... 즉 일주일이 지났을 때 찰칵!
        if (currentDay % 7 == 1)
        {
            // ==========================================
            // 🏦 [주간 은행 결산] 일주일에 한 번 이자 정산!
            // ==========================================
            if (PlayerManager.Instance != null)
            {
                // 1. 주간 예금 이자 (예: 주 1% = 0.01f)
                if (PlayerManager.Instance.bankDeposit > 0)
                {
                    long depositInterest = (long)(PlayerManager.Instance.bankDeposit * 0.01f);
                    PlayerManager.Instance.bankDeposit += depositInterest;
                    Debug.Log($"🏦 [예금 이자] 일주일치 예금 이자 {depositInterest:N0}원이 입금되었습니다!");
                }

                // 2. 주간 대출 이자 (예: 주 5% = 0.05f) -> 꽤 무섭습니다!
                if (PlayerManager.Instance.bankDebt > 0)
                {
                    long debtInterest = (long)(PlayerManager.Instance.bankDebt * 0.05f);
                    PlayerManager.Instance.money -= debtInterest; // 현금 지갑에서 강제 인출!

                    // 가계부의 '지출'에 대출 이자 내역을 추가 (유저가 쓴 돈이니까요!)
                    PlayerManager.Instance.weeklySpent += debtInterest;

                    Debug.Log($"💸 [대출 이자] 일주일치 대출 이자 {debtInterest:N0}원이 빠져나갔습니다!");
                }

                // 돈이 빠져나갔으니 화면 갱신을 위해 신호 한 번 보내줌
                PlayerManager.Instance.AddMoney(0);
            }

            // 💡 8일, 15일, 22일... 즉 일주일이 지났을 때 찰칵!
            if (currentDay % 7 == 1)
            {
                int finishedWeek = (currentDay - 1) / 7; // 예: 8일 차면 1주차 마감!
                PlayerManager.Instance.SaveWeeklyRecord(finishedWeek);
                Debug.Log($"📅 {finishedWeek}주차 가계부가 기록되고 리셋되었습니다!");
            }
        }
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

    // ==========================================
    // 🚀 [디버그/테스트용] 엔딩 직전으로 타임워프!
    // ==========================================
    public void CheatWarpToEnding()
    {
        if (EndingManager.Instance == null) return;

        // 1. 마지막 날의 바로 하루 전날로 세팅 (예: 300일이 끝이면 299일로)
        currentDay = EndingManager.Instance.finalDay - 1;

        // 2. 시간도 강제로 '저녁(장 마감)'으로 맞춰서 바로 [Next Day]를 누를 수 있게 해줍니다.
        currentPhase = DayPhase.Evening;
        currentHour = 18;
        currentMinute = 0;
        isTimeFlowing = false;

        // 3. HUD 글씨 즉시 갱신!
        OnTimeChanged?.Invoke();

        Debug.Log($"🚀 [치트 발동] {currentDay}일차 저녁으로 워프했습니다! [Next Day]를 누르면 엔딩 판별이 시작됩니다.");
    }

    public void ForceUpdateTimeUI()
    {
        OnTimeChanged?.Invoke(); // "시간 바뀌었으니 화면 다시 그려!" 라고 방송함
    }
}