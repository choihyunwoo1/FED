using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PortfolioItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI avgPriceText;
    public TextMeshProUGUI currentPriceText;
    public TextMeshProUGUI yieldText; // 수익률 텍스트
    public string myStockName;

    // 💡 평단가와 현재가를 모두 받아서 계산합니다!
    public void Setup(string stockName, int amount, int avgPrice, int currentPrice)
    {
        myStockName = stockName;

        if (nameText != null) nameText.text = stockName;
        if (amountText != null) amountText.text = amount.ToString() + "Shares";
        if (avgPriceText != null) avgPriceText.text = avgPrice.ToString("N0");
        if (currentPriceText != null) currentPriceText.text = currentPrice.ToString("N0");

        // 📊 수익률 및 손익 계산
        long profit = (long)(currentPrice - avgPrice) * amount; // 총 손익금
        float yieldRate = 0f;
        if (avgPrice > 0)
        {
            yieldRate = ((float)(currentPrice - avgPrice) / avgPrice) * 100f; // 수익률(%)
        }

        // 🎨 한국 주식시장 룰: 오르면 빨간색, 내리면 파란색!
        if (profit > 0)
        {
            yieldText.color = Color.red;
            yieldText.text = $"+{profit:N0} (+{yieldRate:F2}%)";
        }
        else if (profit < 0)
        {
            yieldText.color = Color.blue;
            yieldText.text = $"{profit:N0} ({yieldRate:F2}%)";
        }
        else
        {
            yieldText.color = Color.black; // 본전일 때는 검은색(또는 흰색)
            yieldText.text = $"0 (0.00%)";
        }
    }

    // 💡'전량 매도' 함수!
    public void OnClickSellAllButton()
    {
        // 💡 1. 장이 안 열렸으면 클릭해도 무시해버림!
        if (DayManager.Instance != null && DayManager.Instance.currentPhase != DayPhase.Trading)
        {
            Debug.Log("장이 닫혀 있어 매도할 수 없습니다! (내일 아침 9시 이후 가능)");
            // TODO: 나중에 여기에 화면 중앙 안내 팝업을 띄우면 완벽합니다.
            return;
        }

        // 2. 장이 열려있다면 기존 전량 매도 로직 실행!
        int myAmount = PlayerManager.Instance.GetStockAmount(myStockName);

        if (myAmount > 0)
        {
            int currentPrice = 0;
            foreach (Stock stock in StockManager.Instance.stockList)
            {
                if (stock.stockName == myStockName) { currentPrice = stock.currentPrice; break; }
            }

            long totalMoney = (long)myAmount * currentPrice;

            if (PlayerManager.Instance.SellStock(myStockName, myAmount))
            {
                PlayerManager.Instance.AddMoney(totalMoney);
                Debug.Log($"🚨 패닉 셀 발동! {myStockName} {myAmount}주 전량 매도 ➔ {totalMoney:N0}원 획득!");

                // 전량 매도했으니 이 UI 줄을 화면에서 파괴!
                Destroy(gameObject);
            }
        }
    }
}