using System.Collections.Generic;
using UnityEngine;
using System;

public class StockManager : MonoBehaviour
{
    public static StockManager Instance;

    public List<Stock> stockList = new List<Stock>();

    // 유저가 현재 선택해서 보고 있는 주식의 번호 (기본값 0)
    public int currentStockIndex = 0;

    // 📡 1. 방송국 설립: "주가 변동 알림" 채널입니다.
    public event Action OnStockDataUpdated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 🚀 1. A전자 하나만 있던 시장에 4개의 종목을 상장시킵니다!

        // ① 우량주 (변동성 2% - 아주 안전함)
        Stock stock1 = new Stock { stockName = "A전자", currentPrice = 50000, volatility = 0.02f };
        // 💡 이렇게 뒤에 previousPrice를 currentPrice랑 똑같이 적어주면 시작할 때 에러가 안 납니다!
        // ② 건설주 (변동성 5% - 보통)
        Stock stock2 = new Stock { stockName = "B건설", currentPrice = 15000, volatility = 0.05f };
        // ③ 바이오주 (변동성 10% - 위험함)
        Stock stock3 = new Stock { stockName = "C바이오", currentPrice = 30000, volatility = 0.10f };
        // ④ 코인 (변동성 20% - 야수의 심장!)
        Stock stock4 = new Stock { stockName = "도지코인", currentPrice = 500, volatility = 0.20f };

        stockList.Add(stock1);
        stockList.Add(stock2);
        stockList.Add(stock3);
        stockList.Add(stock4);

        // 2. 4개 종목 모두의 과거 차트를 144개씩 미리 그려둡니다.
        for (int i = 0; i < 144; i++)
        {
            UpdateAllStocks();
        }

        // 💡 3. 144번의 변동이 다 끝난 '최종 가격'을 기준으로, 
        // 1일 차 아침의 전일대비 기준점과 고가/저가를 깔끔하게 0점 세팅해 줍니다!
        foreach (Stock stock in stockList)
        {
            stock.previousPrice = stock.currentPrice;
            stock.todayHigh = stock.currentPrice;
            stock.todayLow = stock.currentPrice;
        }
    }

    public void UpdateAllStocks()
    {
        foreach (Stock stock in stockList)
        {
            GenerateCandleForStock(stock);
        }

        ChartManager chartUI = FindAnyObjectByType<ChartManager>();
        if (chartUI != null && chartUI.gameObject.activeInHierarchy)
        {
            // 🚨 무조건 [0]번이 아니라, "현재 선택된 주식"을 그리도록 수정!
            chartUI.DrawChart(stockList[currentStockIndex]);
        }

        if (OnStockDataUpdated != null)
        {
            OnStockDataUpdated.Invoke();
        }
    }
    private void GenerateCandleForStock(Stock stock)
    {
        int openPrice = stock.currentPrice;
        float randomPercent = UnityEngine.Random.Range(-stock.volatility, stock.volatility);
        int closePrice = openPrice + Mathf.RoundToInt(openPrice * randomPercent);

        closePrice = Mathf.Max(10, closePrice);

        int maxPrice = Mathf.Max(openPrice, closePrice);
        int minPrice = Mathf.Min(openPrice, closePrice);

        int randomShadow = Mathf.RoundToInt(openPrice * UnityEngine.Random.Range(0f, 0.02f));

        int highPrice = maxPrice + randomShadow;
        int lowPrice = minPrice - randomShadow;

        lowPrice = Mathf.Max(10, lowPrice);

        Candle newCandle = new Candle(openPrice, closePrice, highPrice, lowPrice);
        stock.candleHistory.Add(newCandle);

        // 1. 현재가를 방금 계산된 종가(closePrice)로 갱신합니다.
        stock.currentPrice = closePrice;

        // 2. 📈 오늘 하루의 고가(High) 갱신!
        // 방금 찍힌 캔들의 윗꼬리(highPrice)가 기존 오늘 고가보다 높으면 갈아치웁니다.
        if (highPrice > stock.todayHigh)
        {
            stock.todayHigh = highPrice;
        }

        // 3. 📉 오늘 하루의 저가(Low) 갱신!
        // 방금 찍힌 캔들의 밑꼬리(lowPrice)가 기존 오늘 저가보다 낮으면 갈아치웁니다.
        // (단, todayLow가 0일 때는 비교하면 무조건 0이 이기므로 예외 처리 필수!)
        if (stock.todayLow == 0 || lowPrice < stock.todayLow)
        {
            stock.todayLow = lowPrice;
        }
    }

    public void DelistStock(string targetStockName)
    {
        Debug.Log($"🚨 [상장폐지 강제 집행] {targetStockName} 종목이 시장에서 퇴출됩니다!");

        // 💡 [디테일 1] 만약 유저가 지금 '그 주식'의 차트를 보고 있었다면?
        if (stockList.Count > currentStockIndex && stockList[currentStockIndex].stockName == targetStockName)
        {
            // 차트 창을 강제로 닫아버립니다! (비서 호출)
            PopMarket marketUI = FindAnyObjectByType<PopMarket>();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseCurrentPanel();
            }
            // 인덱스를 0번(첫 번째 주식)으로 안전하게 초기화
            currentStockIndex = 0;
        }

        // 1. 시장에서 삭제
        stockList.RemoveAll(s => s.stockName == targetStockName);

        // 2. 가방에서 삭제
        if (PlayerManager.Instance != null && PlayerManager.Instance.portfolio.ContainsKey(targetStockName))
        {
            PlayerManager.Instance.portfolio.Remove(targetStockName);
            Debug.Log($"💸 앗! 보유 중이던 {targetStockName} 주식이 휴지조각이 되었습니다...");
        }

        // 3. UI 업데이트
        if (OnStockDataUpdated != null) OnStockDataUpdated.Invoke();

        // 💡 [디테일 3 연결] 주식이 날아갔으니 혹시 파산했는지 체크해 봅니다!
        if (PlayerManager.Instance != null) PlayerManager.Instance.CheckGameOver();
    }
}