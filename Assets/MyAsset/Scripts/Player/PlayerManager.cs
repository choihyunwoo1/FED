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

    [Header("Item Database (아이템 도감)")]
    // 💡 유니티 인스펙터에서 캔커피, 꽃다발, 시계 등의 스펙을 미리 다 적어둘 리스트!
    public List<ItemData> itemDatabase = new List<ItemData>();

    // 🎒 가방 업그레이드! 이제 int(개수) 대신 OwnedStock(정보 박스)를 담습니다.
    public Dictionary<string, OwnedStock> portfolio = new Dictionary<string, OwnedStock>();

    // 🎁 '잡동사니(선물) 가방'! (이름, 개수)
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

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
    public bool SellStock(string stockName, int amount)
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

    // 🔍 특정 주식을 몇 개나 가지고 있는지 확인하는 함수
    public int GetStockAmount(string stockName)
    {
        // 가방(딕셔너리)에 그 주식이 있다면 개수를 알려줌
        if (portfolio.ContainsKey(stockName))
        {
            return portfolio[stockName].amount;
        }
        return 0; // 없으면 0개 반환!
    }

    // 💡 [디테일 3] 파산(Game Over) 체크 함수
    public void CheckGameOver()
    {
        // 최소 주가(10000원)조차 살 돈이 없고, 가진 주식도 0개라면? = 영원히 복구 불가!
        if (money < 10000 && portfolio.Count == 0)
        {
            Debug.Log("💀 [Game Over] 전 재산을 잃고 파산하셨습니다...");

            // 1. 시간의 흐름을 완전히 멈춤 (사망 선고)
            if (DayManager.Instance != null) DayManager.Instance.isTimeFlowing = false;

            // 2. 우리가 만든 컷씬 프리팹을 재활용해서 게임 오버 화면 띄우기!
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenPanel("PopGameOverCutScene");

                EventCutSceneUI popup = FindAnyObjectByType<EventCutSceneUI>();
                if (popup != null)
                {
                    // 사진은 비워두고(null), 대사만 파산 텍스트로 덮어씌웁니다!
                    popup.SetupStory(null, "<color=red><b>[ 파 산 ]</b></color>\n\n모든 재산을 잃었습니다...\n당신의 투자는 여기서 끝납니다.");
                }
            }
        }
    }

    // 💡 가방에 아이템 넣기/빼기 함수
    public void AddItem(string itemName, int amount)
    {
        if (inventory.ContainsKey(itemName)) inventory[itemName] += amount;
        else inventory.Add(itemName, amount);
        Debug.Log($"🎒 [가방] {itemName}을(를) {amount}개 획득! (총 {inventory[itemName]}개)");
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        return inventory.ContainsKey(itemName) && inventory[itemName] >= amount;
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        if (HasItem(itemName, amount))
        {
            inventory[itemName] -= amount;
            if (inventory[itemName] <= 0) inventory.Remove(itemName); // 다 쓰면 가방에서 버림
        }
    }

    // 🔍 도감에서 아이템 이름으로 스펙(ItemData) 찾아오기
    public ItemData GetItemInfo(string searchName)
    {
        // 도감을 쫙 뒤져서 이름이 똑같은 아이템 정보를 반환합니다.
        return itemDatabase.Find(item => item.itemName == searchName);
    }
}