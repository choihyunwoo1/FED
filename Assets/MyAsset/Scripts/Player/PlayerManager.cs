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

// 💡 과거의 영광(또는 흑역사)을 저장할 데이터 구조체입니다.
[System.Serializable]
public class WeeklyRecord
{
    public int weekNumber; // 몇 주차인지
    public long cash;      // 당시 현금
    public long stockValue;// 당시 주식 평가액
    public long totalAssets;// 당시 총 자산
    public long earned;    // 당시 번 돈
    public long spent;     // 당시 쓴 돈
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    //📚 진짜 장부(1주차, 2주차 기록이 순서대로 쌓임)
    public List<WeeklyRecord> weeklyHistory = new List<WeeklyRecord>();

    [Header("Player Assets")]
    public long money = 1000000;

    [Header("Item Database (아이템 도감)")]
    // 💡 유니티 인스펙터에서 캔커피, 꽃다발, 시계 등의 스펙을 미리 다 적어둘 리스트!
    public List<ItemData> itemDatabase = new List<ItemData>();

    [Header("이번 주 가계부 (Weekly Report)")]
    public long weeklyEarned = 0; // 주식 익절, 이벤트 등으로 번 돈
    public long weeklySpent = 0;  // 상점 지출, 데이트 비용, 주식 손절 등 쓴 돈

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

    public void AddMoney(long amount, bool isIncome = false)
    {
        money += amount;

        // 로비 지원금이나 이벤트로 얻은 '순수 수익'일 때만 가계부에 기록!
        if (isIncome) weeklyEarned += amount;

        UpdateMoneyUI();
    }

    // 💡 돈을 쓸 때 호출하는 함수
    public bool SubtractMoney(long amount, bool isExpense = false)
    {
        if (money >= amount)
        {
            money -= amount;
            // 상점 물건을 사거나 데이트를 한 '순수 지출'일 때만 가계부에 기록!
            if (isExpense) weeklySpent += amount;

            // 돈이 깎였으니 HUD 화면도 새로고침 하라고 신호를 보냅니다!
            UpdateMoneyUI();

            // 💡 돈을 썼으니 파산했는지 매번 체크!
            if (EndingManager.Instance != null) EndingManager.Instance.CheckInstantEndings();

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

        EndingManager.Instance.CheckInstantEndings();

        Debug.Log($"💼 [가방 업데이트] <{stockName}> {portfolio[stockName].amount}주 / 평단가: {portfolio[stockName].averagePrice:N0}원");
    }

    // 💡 주식을 팔았을 때 '수익'과 '손실'을 계산해서 가계부에 적는 로직!
    public bool SellStock(string stockName, int sellAmount)
    {
        if (portfolio.ContainsKey(stockName) && portfolio[stockName].amount >= sellAmount)
        {
            OwnedStock myStock = portfolio[stockName];

            // 1. 내가 살 때 썼던 원금 계산
            long investAmount = (long)sellAmount * myStock.averagePrice;

            // 2. 지금 팔아서 얻는 돈 계산 (현재가는 밖에서 넘어오거나 여기서 찾습니다)
            Stock s = StockManager.Instance.stockList.Find(x => x.stockName == stockName);
            long sellIncome = (long)sellAmount * s.currentPrice;

            // 3. 차익(순수익) 계산!
            long profit = sellIncome - investAmount;

            if (profit > 0)
                weeklyEarned += profit; // 이득 봤으면 번 돈에 추가!
            else if (profit < 0)
                weeklySpent += System.Math.Abs(profit); // 손절 쳤으면 쓴 돈(손해)에 추가!

            // ... (기존 주식 빼는 로직 유지) ...
            myStock.amount -= sellAmount;
            if (myStock.amount <= 0) portfolio.Remove(stockName);

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
    public void AddItem(string itemName, int amount = 1)
    {
        if (inventory.ContainsKey(itemName))
            inventory[itemName] += amount;
        else
            inventory.Add(itemName, amount);

        Debug.Log($"🎒 가방에 [{itemName}] {amount}개가 들어왔습니다!");
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        if (inventory.ContainsKey(itemName))
        {
            return inventory[itemName] >= amount;
        }
        return false;
    }

    // 💡 3. 선물을 주거나 사용할 때 가방에서 빼는 함수
    public void RemoveItem(string itemName, int amount = 1)
    {
        if (inventory.ContainsKey(itemName) && inventory[itemName] >= amount)
        {
            inventory[itemName] -= amount;

            // 다 썼으면 빈 껍데기는 가방에서 버리기!
            if (inventory[itemName] <= 0)
                inventory.Remove(itemName);
        }
    }

    // 🔍 도감에서 아이템 이름으로 스펙(ItemData) 찾아오기
    public ItemData GetItemInfo(string searchName)
    {
        // 도감을 쫙 뒤져서 이름이 똑같은 아이템 정보를 반환합니다.
        return itemDatabase.Find(item => item.itemName == searchName);
    }

    // 💡 일주일이 끝날 때, 이 함수를 불러서 현재 상태를 찰칵! 찍어 저장합니다.
    public void SaveWeeklyRecord(int weekNum)
    {
        long currentCash = money;
        long currentStockValue = 0;

        // 당시 주식 가치 계산
        if (StockManager.Instance != null)
        {
            foreach (var kvp in portfolio)
            {
                Stock s = StockManager.Instance.stockList.Find(x => x.stockName == kvp.Key);
                if (s != null) currentStockValue += (long)kvp.Value.amount * s.currentPrice;
            }
        }

        WeeklyRecord record = new WeeklyRecord
        {
            weekNumber = weekNum,
            cash = currentCash,
            stockValue = currentStockValue,
            totalAssets = currentCash + currentStockValue,
            earned = weeklyEarned,
            spent = weeklySpent
        };

        weeklyHistory.Add(record);

        // 사진 다 찍었으니 다음 주를 위해 실시간 수익/지출은 0으로 리셋!
        weeklyEarned = 0;
        weeklySpent = 0;
    }

    // ==========================================
    // 💡 [진 엔딩용] 전 재산 사회 환원 (기부) 시스템
    // ==========================================
    public void DonateAllWealth()
    {
        // 1. 가진 돈을 0원으로 만듭니다.
        long donatedMoney = money;
        money = 0;

        // (선택) 가계부에 '지출'로 기록하고 싶다면 주석 해제!
        // weeklySpent += donatedMoney; 

        // 2. 가진 주식도 모두 처분(기부)합니다.
        portfolio.Clear();

        // 3. UI 새로고침
        UpdateMoneyUI();

        Debug.Log($"🕊️ [무소유] 플레이어가 {donatedMoney:N0}원과 모든 주식을 기부했습니다...");
    }
}