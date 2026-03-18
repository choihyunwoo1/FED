using System.Collections.Generic;

[System.Serializable]
public class Stock
{
    public string stockName;      // 주식 이름
    public int currentPrice;      // 현재가 (실시간 표시용)
    public int previousPrice; // 어제 장 마감할 때의 가격
    public int todayHigh;     // 오늘 하루 중 가장 높았던 가격
    public int todayLow;      // 오늘 하루 중 가장 낮았던 가격

    // 추세 제어용 변수
    public float trend = 0f;      // +면 상승세, -면 하락세
    public float volatility = 0.05f; // 기본 변동성 (5%)

    // 캔들 데이터들의 목록 (보관함)
    public List<Candle> candleHistory = new List<Candle>();

    public bool isFavorite = false;
}