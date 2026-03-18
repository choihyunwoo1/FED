using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ChartManager : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("UI References")]
    public RectTransform chartContainer;
    public RectTransform chartViewport;
    public GameObject candlePrefab;
    public ScrollRect scrollRect;

    [Header("Chart Settings")]
    public float pricePerPixel = 10f;
    public float viewBottomPrice = 9000f;

    // 💡 줌 아웃했을 때 텅 비어 보이지 않도록, 캔들을 최대 500개까지 미리 그립니다!
    public int maxVisibleCandles = 500;

    public float verticalDragSensitivity = 0.1f;
    [Range(0f, 1f)]
    public float startScrollPosition = 1f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minPricePerPixel = 0.1f;
    public float maxPricePerPixel = 100f;

    // 💡 가로 줌(Zoom) 설정 추가!
    public float minXScale = 0.2f; // 가로 최대 축소 (멀리서 보기)
    public float maxXScale = 3f;   // 가로 최대 확대 (가까이서 보기)
    private float currentXScale = 1f;

    [Header("Price Line Settings")]
    public GameObject priceLinePrefab;
    private RectTransform activePriceLine;

    public void DrawChart(Stock stock)
    {
        foreach (Transform child in chartContainer) Destroy(child.gameObject);

        int startIndex = Mathf.Max(0, stock.candleHistory.Count - maxVisibleCandles);
        List<Candle> candlesToDraw = stock.candleHistory.GetRange(startIndex, stock.candleHistory.Count - startIndex);

        if (candlesToDraw.Count == 0) return;

        float chartHeight = chartViewport.rect.height;

        foreach (Candle c in candlesToDraw)
        {
            GameObject newCandle = Instantiate(candlePrefab, chartContainer);
            UICandle ui = newCandle.GetComponent<UICandle>();
            ui.SetupCandleUI(c, viewBottomPrice, pricePerPixel, chartHeight);
        }

        UpdatePriceLine(stock);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float priceDelta = eventData.delta.y * pricePerPixel * verticalDragSensitivity;
        viewBottomPrice -= priceDelta;
        RefreshChart();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float zoomAmount = eventData.scrollDelta.y * zoomSpeed;

        // 1. 세로 줌 (가격 픽셀 조절)
        if (zoomAmount > 0) pricePerPixel /= (1f + zoomAmount);
        else pricePerPixel *= (1f - zoomAmount);
        pricePerPixel = Mathf.Clamp(pricePerPixel, minPricePerPixel, maxPricePerPixel);

        // 2. 가로 줌 (차트 도화지의 X축 크기를 조절)
        if (zoomAmount > 0) currentXScale *= (1f + zoomAmount);
        else currentXScale /= (1f - zoomAmount);
        currentXScale = Mathf.Clamp(currentXScale, minXScale, maxXScale);

        // 💡 캔들을 담고 있는 도화지의 가로 길이를 실제로 줄이거나 늘립니다!
        chartContainer.localScale = new Vector3(currentXScale, 1f, 1f);

        RefreshChart();
    }

    private void RefreshChart()
    {
        if (StockManager.Instance != null && StockManager.Instance.stockList.Count > 0)
        {
            DrawChart(StockManager.Instance.stockList[StockManager.Instance.currentStockIndex]);
        }
    }

    private void OnEnable()
    {
        if (StockManager.Instance != null && StockManager.Instance.stockList.Count > 0)
        {
            Stock s = StockManager.Instance.stockList[StockManager.Instance.currentStockIndex];
            float viewRangePrice = chartViewport.rect.height * pricePerPixel;
            viewBottomPrice = s.currentPrice - (viewRangePrice * 0.5f);

            // 창을 열 때마다 가로 스케일 1배율로 초기화
            currentXScale = 1f;
            chartContainer.localScale = new Vector3(currentXScale, 1f, 1f);

            DrawChart(s);
            StartCoroutine(SnapToRightCoroutine());
        }
    }

    private IEnumerator SnapToRightCoroutine()
    {
        if (scrollRect == null) yield break;
        yield return null;
        scrollRect.horizontalNormalizedPosition = startScrollPosition;
    }

    public void UpdatePriceLine(Stock stock)
    {
        if (activePriceLine == null)
        {
            GameObject obj = Instantiate(priceLinePrefab, chartViewport);
            activePriceLine = obj.GetComponent<RectTransform>();
            activePriceLine.SetAsLastSibling();
        }

        float yPos = (stock.currentPrice - viewBottomPrice) / pricePerPixel;
        activePriceLine.anchoredPosition = new Vector2(0, yPos);

        var txt = activePriceLine.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = stock.currentPrice.ToString("N0");

        float chartHeight = chartViewport.rect.height;
        activePriceLine.gameObject.SetActive(yPos >= 0 && yPos <= chartHeight);
    }
}