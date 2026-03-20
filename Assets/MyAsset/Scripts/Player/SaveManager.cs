using System.Collections.Generic;
using System.IO; // 💡 파일 입출력을 위해 꼭 필요합니다!
using UnityEngine;

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

        // 💡 6. 가계부 정보도 통에 담습니다!
        if (PlayerManager.Instance != null)
        {
            data.weeklyEarned = PlayerManager.Instance.weeklyEarned;
            data.weeklySpent = PlayerManager.Instance.weeklySpent;
            // 리스트도 그대로 복사해서 담아줍니다!
            data.weeklyHistory = new List<WeeklyRecord>(PlayerManager.Instance.weeklyHistory);
        }

        // 💡 7. 거버넌스 정보 저장
        if (GovernanceManager.Instance != null)
        {
            data.globalPlayerInfluence = GovernanceManager.Instance.globalPlayerInfluence;

            // 딕셔너리 분해해서 담기
            foreach (var kvp in GovernanceManager.Instance.npcLikability)
            {
                data.govNpcNames.Add(kvp.Key);
                data.govNpcLikabilities.Add(kvp.Value);
            }

            // 내일 터질 사건들(SO)을 '이름'만 추출해서 담기!
            foreach (var pEvent in GovernanceManager.Instance.tomorrowEvents)
            {
                data.savedTomorrowEvents.Add(new SaveData.SavedPendingEvent
                {
                    npcName = pEvent.npc.npcName,
                    questName = pEvent.quest.name, // 💡 파일 이름 저장
                    isSuccess = pEvent.isSuccess
                });
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

        // 💡 6. 가계부 정보를 게임에 다시 덮어씌웁니다!
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.weeklyEarned = data.weeklyEarned;
            PlayerManager.Instance.weeklySpent = data.weeklySpent;
            PlayerManager.Instance.weeklyHistory = new List<WeeklyRecord>(data.weeklyHistory);
        }

        // 💡 7. 거버넌스 정보 복구
        if (GovernanceManager.Instance != null)
        {
            GovernanceManager.Instance.globalPlayerInfluence = data.globalPlayerInfluence;

            // 딕셔너리 조립
            GovernanceManager.Instance.npcLikability.Clear();
            for (int i = 0; i < data.govNpcNames.Count; i++)
            {
                GovernanceManager.Instance.npcLikability.Add(data.govNpcNames[i], data.govNpcLikabilities[i]);
            }

            // 내일 터질 사건들 원본 도감에서 찾아오기!
            GovernanceManager.Instance.tomorrowEvents.Clear();
            foreach (var savedEvt in data.savedTomorrowEvents)
            {
                // 도감에서 NPC 먼저 찾고 -> 그 NPC의 퀘스트 목록에서 퀘스트 찾기
                NpcData foundNpc = GovernanceManager.Instance.allNpcDatabase.Find(x => x.npcName == savedEvt.npcName);
                if (foundNpc != null)
                {
                    QuestData foundQuest = foundNpc.possibleQuests.Find(x => x.name == savedEvt.questName);

                    if (foundQuest != null)
                    {
                        // 찾은 원본 데이터로 사건 수첩 완벽 복구!
                        GovernanceManager.Instance.tomorrowEvents.Add(new GovernanceManager.PendingEvent
                        {
                            npc = foundNpc,
                            quest = foundQuest,
                            isSuccess = savedEvt.isSuccess
                        });
                    }
                }
            }
        }
        Debug.Log($"📂 {slotNumber}번 슬롯 불러오기 완료!");
    }
}