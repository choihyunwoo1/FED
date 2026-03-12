using System.Collections.Generic;

[System.Serializable]
public class Stock
{
    public string stockName;      // 주식 이름
    public int currentPrice;      // 현재가 (실시간 표시용)

    // 추세 제어용 변수
    public float trend = 0f;      // +면 상승세, -면 하락세
    public float volatility = 0.05f; // 기본 변동성 (5%)

    // 캔들 데이터들의 목록 (보관함)
    public List<Candle> candleHistory = new List<Candle>();

    public bool isFavorite = false;
}