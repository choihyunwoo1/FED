using UnityEngine;
using UnityEngine.UI;

public class UICandle : MonoBehaviour
{
    public Image bodyImage;
    public Image shadowImage;
    public RectTransform bodyRect;
    public RectTransform shadowRect;

    public Color bullishColor = Color.red;
    public Color bearishColor = Color.blue;

    // 💡 이제 maxPrice, minPrice를 받지 않고 pricePerPixel 방식을 사용합니다.
    public void SetupCandleUI(Candle candle, float viewBottomPrice, float pricePerPixel, float chartHeight)
    {
        // 1. 색상 결정
        Color candleColor = candle.closePrice >= candle.openPrice ? bullishColor : bearishColor;
        bodyImage.color = candleColor;
        shadowImage.color = candleColor;

        // 2. 가격을 화면 좌표(Y)로 변환 (현재가 선과 똑같은 공식!)
        float openY = (candle.openPrice - viewBottomPrice) / pricePerPixel;
        float closeY = (candle.closePrice - viewBottomPrice) / pricePerPixel;
        float highY = (candle.highPrice - viewBottomPrice) / pricePerPixel;
        float lowY = (candle.lowPrice - viewBottomPrice) / pricePerPixel;

        // 3. 몸통 위치와 높이 계산
        float bodyBottom = Mathf.Min(openY, closeY);
        float bodyTop = Mathf.Max(openY, closeY);
        float bodyHeight = Mathf.Max(Mathf.Abs(openY - closeY), 2f); // 너무 작으면 안보이니까 최소 2픽셀

        // 4. 그림자(꼬리) 위치와 높이 계산
        float shadowHeight = Mathf.Max(Mathf.Abs(highY - lowY), 2f);

        // 5. 실제 UI에 적용
        bodyRect.anchoredPosition = new Vector2(0, bodyBottom);
        bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, bodyHeight);

        shadowRect.anchoredPosition = new Vector2(0, lowY);
        shadowRect.sizeDelta = new Vector2(shadowRect.sizeDelta.x, shadowHeight);
    }
}