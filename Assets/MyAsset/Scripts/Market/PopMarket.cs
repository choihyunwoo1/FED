using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopMarket : MonoBehaviour
{
    [Header("UI 연결 (필수)")]
    public Image backgroundImage;
    public TMP_InputField amountInputField;
    public TextMeshProUGUI tradeTitleText;
    public TextMeshProUGUI currentPriceText;
    public TextMeshProUGUI totalPriceText;

    [Header("디자인 설정 (선택)")]
    public Color buyPanelColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color sellPanelColor = new Color(0.2f, 0.7f, 0.2f, 1f);

    private bool isBuyMode = true;
    private int currentAmount = 1;
    private int maxAmount = 0;
    private int unitPrice = 0;
    private Stock currentStock;

    // 💡 [수정 1] Start 대신 OnEnable 사용! (창이 열릴 때마다 무조건 실행됨)
    private void OnEnable()
    {
        if (amountInputField != null)
        {
            amountInputField.onEndEdit.RemoveAllListeners();
            amountInputField.onEndEdit.AddListener(OnQuantityInputFieldEndEdit);
        }

        // 💡 [수정] 버튼 클릭 이벤트 대신, 순수하게 '매수 모드 세팅'만 부르도록 변경!
        SetupTradePanel(true);
    }

    // 💡 [수정 3] 창이 켜져 있는 동안 매 프레임마다 실시간으로 가격을 검사합니다!
    private void Update()
    {
        if (currentStock != null && StockManager.Instance != null)
        {
            int realTimePrice = currentStock.currentPrice; // 진짜 현재가

            // 만약 가격이 예전과 달라졌다면? ➔ 즉시 UI 새로고침!
            if (unitPrice != realTimePrice)
            {
                unitPrice = realTimePrice;

                // 가격이 변했으니 내가 살 수 있는 최대 주식 수도 바뀜!
                if (isBuyMode)
                {
                    long maxBuy = PlayerManager.Instance.money / unitPrice;
                    maxAmount = (int)Mathf.Clamp(maxBuy, 0, 999999);
                }

                // 가격이 올라서 최대 수량이 내 현재 입력값보다 작아지면 수량 강제 조정
                if (currentAmount > maxAmount) currentAmount = maxAmount;

                UpdateTradeUI();
            }
        }
    }

    // 🔴 [매수(Buy)] 버튼
    public void OnClickOpenBuyMode()
    {
        if (isBuyMode && gameObject.activeInHierarchy)
            OnClickConfirmTrade();
        else
            SetupTradePanel(true);
    }

    // 🔵 [매도(Sell)] 버튼
    public void OnClickOpenSellMode()
    {
        if (!isBuyMode && gameObject.activeInHierarchy)
            OnClickConfirmTrade();
        else
            SetupTradePanel(false);
    }

    private void SetupTradePanel(bool buyMode)
    {
        if (StockManager.Instance == null || StockManager.Instance.stockList.Count == 0) return;

        isBuyMode = buyMode;
        currentStock = StockManager.Instance.stockList[StockManager.Instance.currentStockIndex];
        unitPrice = currentStock.currentPrice;

        if (isBuyMode)
        {
            tradeTitleText.text = "<color=red>매수하기 (Buy)</color>";
            if (backgroundImage != null) backgroundImage.color = buyPanelColor;

            long maxBuy = PlayerManager.Instance.money / unitPrice;
            maxAmount = (int)Mathf.Clamp(maxBuy, 0, 999999);
        }
        else
        {
            tradeTitleText.text = "<color=blue>매도하기 (Sell)</color>";
            if (backgroundImage != null) backgroundImage.color = sellPanelColor;

            maxAmount = 0;
            if (PlayerManager.Instance.portfolio.ContainsKey(currentStock.stockName))
            {
                maxAmount = PlayerManager.Instance.portfolio[currentStock.stockName].amount;
            }
        }

        // 수량 안전장치
        if (currentAmount > maxAmount) currentAmount = maxAmount;
        if (currentAmount <= 0 && maxAmount > 0) currentAmount = 1;
        if (maxAmount == 0) currentAmount = 0;

        if (amountInputField != null) amountInputField.text = currentAmount.ToString();
        UpdateTradeUI();
    }

    // ⌨️ 유저가 키보드로 입력하고 엔터를 치거나 클릭을 해제할 때 자동 실행됨!
    public void OnQuantityInputFieldEndEdit(string input)
    {
        // 빈칸 방지 및 안전한 숫자 변환 (.Trim()으로 공백 에러 방지)
        if (!int.TryParse(input.Trim(), out int inputAmount)) inputAmount = 1;

        if (inputAmount < 0) inputAmount = 0;
        if (maxAmount > 0 && inputAmount == 0) inputAmount = 1;

        if (inputAmount > maxAmount)
        {
            inputAmount = maxAmount;
            Debug.LogWarning($"❌ 최댓값({maxAmount}) 초과로 자동 조정됨.");
        }

        currentAmount = inputAmount;
        if (amountInputField != null) amountInputField.text = currentAmount.ToString();
        UpdateTradeUI();
    }

    public void AddAmount(int amount)
    {
        if (amount < 0) return;

        currentAmount += amount;
        if (currentAmount > maxAmount) currentAmount = maxAmount;

        if (amountInputField != null) amountInputField.text = currentAmount.ToString();
        UpdateTradeUI();
    }

    public void SetMaxAmount()
    {
        currentAmount = maxAmount;
        if (amountInputField != null) amountInputField.text = currentAmount.ToString();
        UpdateTradeUI();
    }

    private void UpdateTradeUI()
    {
        if (currentPriceText != null) currentPriceText.text = $"현재가: {unitPrice:N0}원";

        long total = (long)currentAmount * unitPrice;
        if (totalPriceText != null)
            totalPriceText.text = isBuyMode ? $"총 결제액: {total:N0}원" : $"총 판매액: {total:N0}원";
    }

    public void OnClickConfirmTrade()
    {
        // 💡 [수정 4] 버튼 누르기 직전의 텍스트를 한 번 더 안전하게 파싱합니다!
        if (amountInputField != null && int.TryParse(amountInputField.text.Trim(), out int lastInput))
        {
            currentAmount = lastInput;
        }

        if (currentAmount > maxAmount) currentAmount = maxAmount;
        if (maxAmount > 0 && currentAmount <= 0) currentAmount = 1;

        if (currentAmount <= 0)
        {
            Debug.LogWarning("❌ 수량이 0개라 거래할 수 없습니다.");
            return;
        }

        long totalAmountStr = (long)currentAmount * unitPrice;

        if (isBuyMode)
        {
            if (PlayerManager.Instance.SubtractMoney(totalAmountStr))
            {
                PlayerManager.Instance.AddStock(currentStock.stockName, currentAmount, unitPrice);
                Debug.Log($"🎉 [매수 성공] {currentStock.stockName} {currentAmount}주 구매!");
            }
        }
        else
        {
            if (PlayerManager.Instance.SellStock(currentStock.stockName, currentAmount))
            {
                PlayerManager.Instance.AddMoney(totalAmountStr);
                Debug.Log($"💸 [매도 성공] {currentStock.stockName} {currentAmount}주 판매!");
            }
        }

        SetupTradePanel(isBuyMode);
    }

    public void OnClickCloseButton()
    {
        UIManager.Instance.CloseCurrentPanel();
    }
}