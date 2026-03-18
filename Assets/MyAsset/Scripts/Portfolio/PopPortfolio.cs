using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PopPortfolio : MonoBehaviour
{
    [Header("UI 연결: 페이지 이동")]
    public TextMeshProUGUI weekTitleText; // 예: "1주차 리포트", "[진행중] 3주차 리포트"
    public GameObject leftButton;         // 왼쪽 화살표 버튼
    public GameObject rightButton;        // 오른쪽 화살표 버튼

    [Header("UI 연결: 자산 현황")] //"지금 내 전 재산이 얼마야?"
    public TextMeshProUGUI cashText; //(현금): 말 그대로 지금 당장 붕어빵을 사 먹을 수 있는 순수 현금!
    public TextMeshProUGUI stockValueText; //(주식 평가액): 내가 가진 주식들을 "지금 당장 전부 내다 팔았을 때" 받을 수 있는 돈
    public TextMeshProUGUI totalAssetsText; //(총 자산): 위 두 개를 더한 값입니다. 은행 대출 심사(?) 받을 때 내미는 진정한 내 재산의 크기

    [Header("UI 연결: 가계부")] //"이번 주에 돈 관리를 어떻게 했지?"
    public TextMeshProUGUI weeklyEarnedText; //(번 돈): 이번 주에 **'순수하게 이득 본 금액'**입니다.
    public TextMeshProUGUI weeklySpentText; //(쓴 돈): 이번 주에 **'내 주머니에서 빠져나간 모든 금액'**입니다
    public TextMeshProUGUI netIncomeText; //(순이익): (번 돈 - 쓴 돈) 입니다.

    // 💡 내가 지금 보고 있는 페이지 번호 (0 = 1주차 과거, 1 = 2주차 과거... 최대값 = 현재 진행중인 주)
    private int currentViewIndex = 0;

    private void OnEnable()
    {
        // 창을 열 때는 무조건 '가장 최신(현재 진행 중인 주차)'을 보여줍니다.
        currentViewIndex = PlayerManager.Instance.weeklyHistory.Count;
        UpdatePortfolioUI();
    }

    // ◀️ 왼쪽 버튼 (과거로 가기)
    public void OnClickLeft()
    {
        if (currentViewIndex > 0)
        {
            currentViewIndex--;
            UpdatePortfolioUI();
        }
    }

    // ▶️ 오른쪽 버튼 (최신으로 오기)
    public void OnClickRight()
    {
        if (currentViewIndex < PlayerManager.Instance.weeklyHistory.Count)
        {
            currentViewIndex++;
            UpdatePortfolioUI();
        }
    }

    private void UpdatePortfolioUI()
    {
        if (PlayerManager.Instance == null) return;

        int historyCount = PlayerManager.Instance.weeklyHistory.Count;
        bool isCurrentWeek = (currentViewIndex == historyCount); // 지금 보는 게 이번 주인가?
        int displayWeek = currentViewIndex + 1; // 화면에 띄울 주차 숫자

        long displayCash = 0;
        long displayStockValue = 0;
        long displayTotalAssets = 0;
        long displayEarned = 0;
        long displaySpent = 0;

        // 💡 1. 현재 진행 중인 주차라면? (실시간 데이터 싹쓸이)
        if (isCurrentWeek)
        {
            weekTitleText.text = $"[진행중] {displayWeek}주차 리포트";

            displayCash = PlayerManager.Instance.money;
            if (StockManager.Instance != null)
            {
                foreach (var kvp in PlayerManager.Instance.portfolio)
                {
                    Stock s = StockManager.Instance.stockList.Find(x => x.stockName == kvp.Key);
                    if (s != null) displayStockValue += (long)kvp.Value.amount * s.currentPrice;
                }
            }
            displayTotalAssets = displayCash + displayStockValue;
            displayEarned = PlayerManager.Instance.weeklyEarned;
            displaySpent = PlayerManager.Instance.weeklySpent;

            // 최신 페이지니까 오른쪽 이동 버튼 숨기기
            if (rightButton != null) rightButton.SetActive(false);
        }
        // 💡 2. 과거의 주차라면? (기록해둔 장부에서 데이터 꺼내오기)
        else
        {
            weekTitleText.text = $"[마감] {displayWeek}주차 리포트";

            WeeklyRecord record = PlayerManager.Instance.weeklyHistory[currentViewIndex];
            displayCash = record.cash;
            displayStockValue = record.stockValue;
            displayTotalAssets = record.totalAssets;
            displayEarned = record.earned;
            displaySpent = record.spent;

            // 최신 페이지가 아니니까 오른쪽 이동 버튼 켜기
            if (rightButton != null) rightButton.SetActive(true);
        }

        // 과거로 가는 버튼은 1주차(0번 인덱스)일 때만 숨기기
        if (leftButton != null) leftButton.SetActive(currentViewIndex > 0);

        // 💡 3. 화면에 예쁘게 글씨 뿌려주기
        if (cashText != null) cashText.text = $"{displayCash:N0}원";
        if (stockValueText != null) stockValueText.text = $"{displayStockValue:N0}원";
        if (totalAssetsText != null) totalAssetsText.text = $"{displayTotalAssets:N0}원";

        if (weeklyEarnedText != null) weeklyEarnedText.text = $"<color=red>+{displayEarned:N0}원</color>";
        if (weeklySpentText != null) weeklySpentText.text = $"<color=blue>-{displaySpent:N0}원</color>";

        // 순이익 계산 및 색상 적용
        long netIncome = displayEarned - displaySpent;
        if (netIncomeText != null)
        {
            string colorTag = netIncome > 0 ? "<color=red>" : (netIncome < 0 ? "<color=blue>" : "<color=black>");
            string sign = netIncome > 0 ? "+" : "";
            netIncomeText.text = $"{colorTag}{sign}{netIncome:N0}원</color>";
        }
    }

    public void OnClickClose()
    {
        // UIManager를 쓰지 않고, 자기 자신만 깔끔하게 화면에서 숨깁니다.
        gameObject.SetActive(false);
    }
}