// PortfolioUI.cs
using UnityEngine;
using System.Collections.Generic; // Dictionary를 쓰기 위해 필요

public class PortfolioUI : MonoBehaviour
{
    public GameObject itemPrefab;      // 1단계에서 만든 'PortfolioItemPrefab'
    public Transform contentTransform; // 목록들이 들어갈 부모 객체 (보통 Scroll View의 Content)

    // 💡 [디테일 2] "보유 주식이 없습니다" 텍스트
    public GameObject emptyStateMessage;

    [Header("가계부 팝업 연결")]
    public GameObject popTotalPortfolioPanel;

    // 💡 창이 켜질 때: 신호 연결(구독) 및 즉시 새로고침
    private void OnEnable()
    {
        // StockManager가 주가를 갱신할 때마다 RefreshPortfolio 함수를 실행하라고 등록!
        if (StockManager.Instance != null)
        {
            StockManager.Instance.OnStockDataUpdated += RefreshPortfolio;
        }

        // 열리자마자 한 번은 그려줘야죠!
        RefreshPortfolio();
    }

    // 💡 창이 꺼질 때: 신호 끊기(구독 취소) -> 중요! 안 하면 에러 남!
    private void OnDisable()
    {
        if (StockManager.Instance != null)
        {
            StockManager.Instance.OnStockDataUpdated -= RefreshPortfolio;
        }
    }
    public void RefreshPortfolio()
    {
        foreach (Transform child in contentTransform) Destroy(child.gameObject);

        if (PlayerManager.Instance == null || PlayerManager.Instance.portfolio.Count == 0)
        {
            // 빈 화면 안내 문구 켜기
            if (emptyStateMessage != null) emptyStateMessage.SetActive(true);
            return; // 여기서 함수 종료
        }

        // 가방에 주식이 있다면 안내 문구 끄기
        if (emptyStateMessage != null) emptyStateMessage.SetActive(false);

        foreach (KeyValuePair<string, OwnedStock> item in PlayerManager.Instance.portfolio)
        {
            string stockName = item.Key;
            int stockAmount = item.Value.amount;
            int avgPrice = item.Value.averagePrice; // 💡 가방에서 평단가 꺼내기!

            // 💡 StockManager에게 "이 주식 지금 얼마야?" 라고 물어봐서 현재가 가져오기!
            int currentPrice = 0;
            Stock liveStock = StockManager.Instance.stockList.Find(s => s.stockName == stockName);
            if (liveStock != null)
            {
                currentPrice = liveStock.currentPrice;
            }

            GameObject go = Instantiate(itemPrefab, contentTransform);
            PortfolioItem uiItem = go.GetComponent<PortfolioItem>();

            // 프리팹에게 4가지 정보를 모두 던져줍니다!
            uiItem.Setup(stockName, stockAmount, avgPrice, currentPrice);
        }
    }

    public void OnClickOpenTotalPortfolio()
    {
        if (popTotalPortfolioPanel != null)
        {
            // 비서(UIManager) 모르게 직접 창을 켭니다!
            popTotalPortfolioPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("❌ 가계부 패널이 연결되지 않았습니다!");
        }
    }

}