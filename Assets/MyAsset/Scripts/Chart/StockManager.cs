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
    }

    public void UpdateAllStocks()
    {
        foreach (Stock stock in stockList)
        {
            GenerateCandleForStock(stock);
        }

        ChartManager chartUI = FindObjectOfType<ChartManager>();
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

        int maxPrice = Mathf.Max(openPrice, closePrice);
        int minPrice = Mathf.Min(openPrice, closePrice);

        int randomShadow = Mathf.RoundToInt(openPrice * UnityEngine.Random.Range(0f, 0.02f));

        int highPrice = maxPrice + randomShadow;
        int lowPrice = minPrice - randomShadow;

        Candle newCandle = new Candle(openPrice, closePrice, highPrice, lowPrice);
        stock.candleHistory.Add(newCandle);
        stock.currentPrice = closePrice;
    }
}