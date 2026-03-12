using UnityEngine;
using TMPro;

public class PortfolioItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI avgPriceText;
    public TextMeshProUGUI currentPriceText;
    public TextMeshProUGUI yieldText; // 수익률 텍스트

    // 💡 평단가와 현재가를 모두 받아서 계산합니다!
    public void Setup(string stockName, int amount, int avgPrice, int currentPrice)
    {
        nameText.text = stockName;
        amountText.text = amount.ToString() + "Shares";
        avgPriceText.text = avgPrice.ToString("N0");
        currentPriceText.text = currentPrice.ToString("N0");

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
}