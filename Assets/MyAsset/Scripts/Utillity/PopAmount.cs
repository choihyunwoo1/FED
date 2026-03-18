using UnityEngine;
using TMPro;
using System;

public class PopAmount : MonoBehaviour
{
    public static PopAmount Instance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI totalPriceText;

    private int currentAmount = 1;
    private int maxAmount = 99;
    private int unitPrice = 0;

    // 💡 이번엔 "몇 개 골랐는지(int)"를 전달하는 Action입니다!
    private Action<int> onConfirmCallback;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ShowPopup(string title, int price, int maxQty, Action<int> onConfirm)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        unitPrice = price;
        maxAmount = maxQty;
        currentAmount = 1; // 열릴 때 무조건 1개로 초기화
        onConfirmCallback = onConfirm;

        UpdateUI();
    }

    public void OnClickPlus()
    {
        if (currentAmount < maxAmount) currentAmount++;
        UpdateUI();
    }

    public void OnClickMinus()
    {
        if (currentAmount > 1) currentAmount--;
        UpdateUI();
    }

    private void UpdateUI()
    {
        amountText.text = currentAmount.ToString();
        // 💡 가격이 커질 수 있으니 long으로 계산!
        long total = (long)currentAmount * unitPrice;
        totalPriceText.text = $"총액: {total:N0}원";
    }

    public void OnClickConfirm()
    {
        if (onConfirmCallback != null) onConfirmCallback.Invoke(currentAmount);
        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}