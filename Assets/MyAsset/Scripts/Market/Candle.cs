[System.Serializable]
public class Candle
{
    public int openPrice;  // 시작 가격 (시가)
    public int closePrice; // 끝난 가격 (종가)
    public int highPrice;  // 최고 가격 (고가)
    public int lowPrice;   // 최저 가격 (저가)

    // 생성자: 새로운 캔들을 만들 때 처음 숫자를 쏙쏙 집어넣어 주는 편리한 기능
    public Candle(int open, int close, int high, int low)
    {
        openPrice = open;
        closePrice = close;
        highPrice = high;
        lowPrice = low;
    }
}