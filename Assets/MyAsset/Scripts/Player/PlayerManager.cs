using UnityEngine;
using System;
using System.Collections.Generic;

// 💡 새로 추가: 가방 안에 넣을 '내 주식 정보 박스'
public class OwnedStock
{
    public int amount;       // 보유 수량
    public int averagePrice; // 매입 단가 (평단가)

    public OwnedStock(int amount, int averagePrice)
    {
        this.amount = amount;
        this.averagePrice = averagePrice;
    }
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("Player Assets")]
    public long money = 1000000;

    // 🎒 가방 업그레이드! 이제 int(개수) 대신 OwnedStock(정보 박스)를 담습니다.
    public Dictionary<string, OwnedStock> portfolio = new Dictionary<string, OwnedStock>();

    public event Action<long> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(long amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    public bool SubtractMoney(long amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyUI();
            return true;
        }
        return false;
    }

    private void UpdateMoneyUI()
    {
        if (OnMoneyChanged != null) OnMoneyChanged.Invoke(money);
    }

    // 📥 가방에 주식 넣기 (💡 평단가 계산을 위해 '이번에 산 가격(buyPrice)'을 추가로 받습니다)
    public void AddStock(string stockName, int amount, int buyPrice)
    {
        if (portfolio.ContainsKey(stockName))
        {
            // 이미 있는 주식이면 '물타기(가중 평균)' 계산을 합니다!
            OwnedStock stock = portfolio[stockName];

            long totalOldValue = (long)stock.amount * stock.averagePrice; // 기존에 투자한 총액
            long totalNewValue = (long)amount * buyPrice;                 // 이번에 투자한 총액

            stock.amount += amount; // 수량 증가
            // 💡 평단가 계산: (기존 총액 + 새 총액) / 전체 수량
            stock.averagePrice = (int)((totalOldValue + totalNewValue) / stock.amount);
        }
        else
        {
            // 처음 사는 주식이면 새 박스를 만들어서 넣습니다.
            portfolio.Add(stockName, new OwnedStock(amount, buyPrice));
        }

        Debug.Log($"💼 [가방 업데이트] <{stockName}> {portfolio[stockName].amount}주 / 평단가: {portfolio[stockName].averagePrice:N0}원");
    }

    // 📤 가방에서 주식 빼기
    public bool RemoveStock(string stockName, int amount)
    {
        // 정보 박스(OwnedStock) 안의 amount(수량)를 확인해야 합니다.
        if (portfolio.ContainsKey(stockName) && portfolio[stockName].amount >= amount)
        {
            portfolio[stockName].amount -= amount;

            if (portfolio[stockName].amount == 0)
            {
                portfolio.Remove(stockName); // 다 팔았으면 가방에서 이름표 제거
            }
            return true;
        }
        return false;
    }
}