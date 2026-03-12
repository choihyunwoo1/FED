using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MarketItem : MonoBehaviour
{
    public Toggle favoriteToggle;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;

    // 💡 새로 추가된 2가지 텍스트!
    public TextMeshProUGUI changeText; // 등락률 (예: +1,200 (+5.4%))
    public TextMeshProUGUI volumeText; // 거래량 (예: 1,540,230)

    public Button chartButton;

    private Stock myStock;
    private int myIndex;
    private MarketListUI parentUI;

    public void Setup(Stock stock, int index, MarketListUI ui)
    {
        myStock = stock;
        myIndex = index;
        parentUI = ui;

        nameText.text = stock.stockName;
        priceText.text = stock.currentPrice.ToString("N0");

        // 💡 1. 등락률 계산 및 색상 적용 함수 호출!
        CalculateAndSetChangeRate(stock);

        // 💡 2. 거래량 적용 (일단 그럴싸한 가짜 데이터로 채워둡니다)
        // 진짜 거래량을 쓰려면 나중에 Stock.cs에 public long volume; 을 추가하시면 됩니다!
        long fakeVolume = (long)(stock.currentPrice * 2.5f) + UnityEngine.Random.Range(1000, 50000);
        volumeText.text = fakeVolume.ToString("N0");

        favoriteToggle.onValueChanged.RemoveAllListeners();
        favoriteToggle.isOn = stock.isFavorite;
        favoriteToggle.onValueChanged.AddListener(OnFavoriteToggled);

        chartButton.onClick.RemoveAllListeners();
        chartButton.onClick.AddListener(OnClickChart);
    }

    // 📈 등락률(%) 계산기
    private void CalculateAndSetChangeRate(Stock stock)
    {
        // 비교할 '이전 캔들'이 존재하는지 확인 (기록이 2개 이상이어야 함)
        if (stock.candleHistory.Count > 1)
        {
            // 바로 직전 캔들(어제)의 종가(Close Price) 가져오기
            int previousPrice = stock.candleHistory[stock.candleHistory.Count - 2].closePrice;

            // 얼마나 올랐나/내렸나? (현재가 - 어제 종가)
            int changeAmount = stock.currentPrice - previousPrice;
            float changePercent = 0f;

            if (previousPrice > 0)
            {
                changePercent = ((float)changeAmount / previousPrice) * 100f; // 수익률 계산
            }

            // 🎨 색상 입히기 (한국 패치: 상승=빨강, 하락=파랑)
            if (changeAmount > 0)
            {
                changeText.color = Color.red;
                changeText.text = $"+{changeAmount:N0} (+{changePercent:F2}%)";
            }
            else if (changeAmount < 0)
            {
                changeText.color = Color.blue;
                changeText.text = $"{changeAmount:N0} ({changePercent:F2}%)";
            }
            else
            {
                changeText.color = Color.black; // 보합(0)일 때는 검은색이나 흰색
                changeText.text = $"0 (0.00%)";
            }
        }
        else
        {
            // 기록이 없으면 0으로 표시
            changeText.color = Color.black;
            changeText.text = "0 (0.00%)";
        }
    }

    private void OnFavoriteToggled(bool isOn)
    {
        myStock.isFavorite = isOn;
        parentUI.RefreshList();
    }

    private void OnClickChart()
    {
        StockManager.Instance.currentStockIndex = myIndex;
        UIManager.Instance.OpenPanel("PopMarket");
    }
}