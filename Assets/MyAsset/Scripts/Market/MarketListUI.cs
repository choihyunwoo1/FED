using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 💡 C#의 정렬 마법사(LINQ)를 쓰기 위해 반드시 추가!

public class MarketListUI : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentTransform;

    private void OnEnable()
    {
        // 창이 켜질 때와 주가가 변할 때마다 목록 갱신!
        if (StockManager.Instance != null)
        {
            StockManager.Instance.OnStockDataUpdated += RefreshList;
            RefreshList();
        }
    }

    private void OnDisable()
    {
        if (StockManager.Instance != null)
        {
            StockManager.Instance.OnStockDataUpdated -= RefreshList;
        }
    }

    public void RefreshList()
    {
        foreach (Transform child in contentTransform) Destroy(child.gameObject);

        if (StockManager.Instance == null) return;

        // 🚀 C# 마법의 정렬! 
        // 1. 즐겨찾기(isFavorite == true)인 주식을 먼저 정렬하고,
        // 2. 원본 인덱스(번호)를 잃어버리지 않게 같이 묶어서 가져옵니다.
        var sortedStocks = StockManager.Instance.stockList
            .Select((stock, index) => new { stock, index })
            .OrderByDescending(item => item.stock.isFavorite) // 📌 체크된 게 무조건 1등!
            .ThenBy(item => item.index)                       // 나머지는 원래 순서대로
            .ToList();

        // 정렬된 순서대로 프리팹을 찍어냅니다.
        foreach (var item in sortedStocks)
        {
            GameObject go = Instantiate(itemPrefab, contentTransform);
            MarketItem uiItem = go.GetComponent<MarketItem>();

            // 프리팹에게 주식 정보, 원본 번호, 그리고 매니저(나 자신)를 넘겨줍니다.
            uiItem.Setup(item.stock, item.index, this);
        }
    }
}