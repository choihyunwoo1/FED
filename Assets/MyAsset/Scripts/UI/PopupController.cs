using UnityEngine;

public class PopupController : MonoBehaviour
{
    public void OnClickCloseButton()
    {
        UIManager.Instance.CloseCurrentPanel();
    }

    public void OnClickBuyButton()
    {
        // ... (기존 매수 코드 그대로 유지) ...
        if (StockManager.Instance == null || StockManager.Instance.stockList.Count == 0) return;
        Stock currentStock = StockManager.Instance.stockList[StockManager.Instance.currentStockIndex];
        int currentPrice = currentStock.currentPrice;

        if (PlayerManager.Instance.SubtractMoney(currentPrice))
        {
            Debug.Log($"🎉 [매수 성공] {currentStock.stockName} 1주를 {currentPrice:N0}원에 샀습니다!");

            // 🚨 여기 수정! AddStock할 때 현재가(currentPrice)도 같이 넘겨줍니다!
            PlayerManager.Instance.AddStock(currentStock.stockName, 1, currentPrice);
        }
        else
        {
            Debug.LogWarning("❌ [매수 실패] 통장 잔고가 부족합니다.");
        }
    }

    // 📉 새로 추가하는 Sell(매도) 버튼 로직!
    public void OnClickSellButton()
    {
        if (StockManager.Instance == null || StockManager.Instance.stockList.Count == 0) return;

        Stock currentStock = StockManager.Instance.stockList[StockManager.Instance.currentStockIndex];
        int currentPrice = currentStock.currentPrice;

        // 1. PlayerManager에게 "내 가방에서 이 주식 1개 빼줘!" 라고 요청합니다.
        // RemoveStock 함수가 참(true)을 반환하면 가방에 주식이 있어서 성공적으로 뺐다는 뜻입니다.
        if (PlayerManager.Instance.RemoveStock(currentStock.stockName, 1))
        {
            // 2. 가방에서 주식을 빼는 데 성공했으니, 현재가만큼 돈을 입금받습니다!
            PlayerManager.Instance.AddMoney(currentPrice);
            Debug.Log($"💸 [매도 성공] {currentStock.stockName} 1주를 {currentPrice:N0}원에 팔았습니다!");
        }
        else
        {
            // 가방에 주식이 없으면 팔 수 없습니다. (공매도 금지!)
            Debug.LogWarning("❌ [매도 실패] 가방에 팔 수 있는 주식이 없습니다.");
        }
    }
}