using UnityEngine;
using System.IO; // 💡 파일 입출력을 위해 꼭 필요합니다!

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 💡 몇 번 슬롯인지 받아서 파일 경로를 만들어주는 마법의 함수!
    public string GetSaveFilePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotNumber}.json");
    }

    // 💡 해당 슬롯에 세이브 파일이 존재하는지 확인하는 함수 (UI 표기용)
    public bool HasSaveData(int slotNumber)
    {
        return File.Exists(GetSaveFilePath(slotNumber));
    }

    // 💾 [저장하기]
    public void SaveGame(int slotNumber)
    {
        SaveData data = new SaveData();

        // 1. 돈 저장
        data.money = PlayerManager.Instance.money;

        // 2. 인벤토리 저장 (딕셔너리를 2개의 리스트로 분해해서 담기)
        foreach (var item in PlayerManager.Instance.inventory)
        {
            data.inventoryItemNames.Add(item.Key);
            data.inventoryItemAmounts.Add(item.Value);
        }

        // 3. 주식 포트폴리오 저장
        foreach (var stock in PlayerManager.Instance.portfolio)
        {
            data.portfolioStockNames.Add(stock.Key);
            data.portfolioStockAmounts.Add(stock.Value.amount);
            data.portfolioStockAvgPrices.Add(stock.Value.averagePrice);
        }

        // 4. 상점 레벨 저장 (상점 창이 꺼져있어도 찾을 수 있게 옵션 추가)
        ShopUI shop = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shop != null) data.shopLevel = shop.currentShopLevel;

        // 5. NPC 호감도 저장
        if (DateManager.Instance != null)
        {
            foreach (var npc in DateManager.Instance.npcList)
            {
                data.npcAffinities.Add(npc.currentAffinity);
                data.npcRewardsGiven.Add(npc.hasGivenSecret);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(slotNumber), json);
        Debug.Log($"💾 {slotNumber}번 슬롯에 저장 완료!");
    }

    // 📂 [불러오기]
    public void LoadGame(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 1. 돈 복구 (UI 갱신을 위해 AddMoney를 한 번 호출해 주는 꼼수)
        PlayerManager.Instance.money = data.money;
        PlayerManager.Instance.AddMoney(0);

        // 2. 인벤토리 복구
        PlayerManager.Instance.inventory.Clear();
        for (int i = 0; i < data.inventoryItemNames.Count; i++)
        {
            PlayerManager.Instance.inventory.Add(data.inventoryItemNames[i], data.inventoryItemAmounts[i]);
        }

        // 3. 주식 포트폴리오 복구
        PlayerManager.Instance.portfolio.Clear();
        for (int i = 0; i < data.portfolioStockNames.Count; i++)
        {
            PlayerManager.Instance.portfolio.Add(data.portfolioStockNames[i], new OwnedStock(data.portfolioStockAmounts[i], data.portfolioStockAvgPrices[i]));
        }

        // 4. 상점 레벨 복구
        ShopUI shop = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shop != null) shop.currentShopLevel = data.shopLevel;

        // 5. NPC 호감도 복구
        if (DateManager.Instance != null)
        {
            for (int i = 0; i < data.npcAffinities.Count; i++)
            {
                if (i < DateManager.Instance.npcList.Count)
                {
                    DateManager.Instance.npcList[i].currentAffinity = data.npcAffinities[i];
                    DateManager.Instance.npcList[i].hasGivenSecret = data.npcRewardsGiven[i];
                }
            }
        }

        Debug.Log($"📂 {slotNumber}번 슬롯 불러오기 완료!");
    }
}