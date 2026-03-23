using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 💡 [추가됨] 타이틀 씬에서 "이어하기"를 누르고 넘어왔다면 자동으로 로드!
    private void Start()
    {
        // 💡 현재 씬 이름이 "PlayScene"일 때만 자동 로드를 실행하도록 방어막 씌우기!
        if (SceneManager.GetActiveScene().name == "PlayScene")
        {
            if (PlayerPrefs.GetInt("IsLoadGame", 0) == 1)
            {
                int targetSlot = PlayerPrefs.GetInt("LoadSlotNumber", 0);
                Debug.Log($"🚀 {targetSlot}번 슬롯을 자동으로 로드합니다!");
                PlayerPrefs.SetInt("IsLoadGame", 0);
                LoadGame(targetSlot);
            }
        }
    }

    public string GetSaveFilePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotNumber}.json");
    }

    public bool HasSaveData(int slotNumber)
    {
        return File.Exists(GetSaveFilePath(slotNumber));
    }

    // 💾 [저장하기]
    public void SaveGame(int slotNumber)
    {
        SaveData data = new SaveData();

        // 💡 0. [추가됨] 날짜와 시간 저장!
        if (DayManager.Instance != null)
        {
            data.currentDay = DayManager.Instance.currentDay;
            data.currentHour = DayManager.Instance.currentHour;
            data.currentMinute = DayManager.Instance.currentMinute;
            data.currentPhase = DayManager.Instance.currentPhase;
        }

        data.money = PlayerManager.Instance.money;

        foreach (var item in PlayerManager.Instance.inventory)
        {
            data.inventoryItemNames.Add(item.Key);
            data.inventoryItemAmounts.Add(item.Value);
        }

        foreach (var stock in PlayerManager.Instance.portfolio)
        {
            data.portfolioStockNames.Add(stock.Key);
            data.portfolioStockAmounts.Add(stock.Value.amount);
            data.portfolioStockAvgPrices.Add(stock.Value.averagePrice);
        }

        ShopUI shop = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shop != null) data.shopLevel = shop.currentShopLevel;

        if (DateManager.Instance != null)
        {
            foreach (var npc in DateManager.Instance.npcList)
            {
                data.npcAffinities.Add(npc.currentAffinity);
                data.npcRewardsGiven.Add(npc.hasGivenSecret);
            }
        }

        if (PlayerManager.Instance != null)
        {
            data.weeklyEarned = PlayerManager.Instance.weeklyEarned;
            data.weeklySpent = PlayerManager.Instance.weeklySpent;
            data.weeklyHistory = new List<WeeklyRecord>(PlayerManager.Instance.weeklyHistory);
        }

        if (GovernanceManager.Instance != null)
        {
            data.globalPlayerInfluence = GovernanceManager.Instance.globalPlayerInfluence;

            foreach (var kvp in GovernanceManager.Instance.npcLikability)
            {
                data.govNpcNames.Add(kvp.Key);
                data.govNpcLikabilities.Add(kvp.Value);
            }

            foreach (var pEvent in GovernanceManager.Instance.tomorrowEvents)
            {
                data.savedTomorrowEvents.Add(new SaveData.SavedPendingEvent
                {
                    npcName = pEvent.npc.npcName,
                    questName = pEvent.quest.name,
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

        // 💡 0. [추가됨] 날짜와 시간 덮어씌우기!
        if (DayManager.Instance != null)
        {
            DayManager.Instance.currentDay = data.currentDay;
            DayManager.Instance.currentHour = data.currentHour;
            DayManager.Instance.currentMinute = data.currentMinute;
            DayManager.Instance.currentPhase = data.currentPhase;

            // 🚨 시간 강제 갱신으로 화면 글씨(HUD) 업데이트 유도 (꼼수)
            DayManager.Instance.isTimeFlowing = (data.currentPhase == DayPhase.Trading);

            DayManager.Instance.ForceUpdateTimeUI();
        }

        PlayerManager.Instance.money = data.money;
        PlayerManager.Instance.AddMoney(0);

        PlayerManager.Instance.inventory.Clear();
        for (int i = 0; i < data.inventoryItemNames.Count; i++)
        {
            PlayerManager.Instance.inventory.Add(data.inventoryItemNames[i], data.inventoryItemAmounts[i]);
        }

        PlayerManager.Instance.portfolio.Clear();
        for (int i = 0; i < data.portfolioStockNames.Count; i++)
        {
            PlayerManager.Instance.portfolio.Add(data.portfolioStockNames[i], new OwnedStock(data.portfolioStockAmounts[i], data.portfolioStockAvgPrices[i]));
        }

        ShopUI shop = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (shop != null) shop.currentShopLevel = data.shopLevel;

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

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.weeklyEarned = data.weeklyEarned;
            PlayerManager.Instance.weeklySpent = data.weeklySpent;
            PlayerManager.Instance.weeklyHistory = new List<WeeklyRecord>(data.weeklyHistory);
        }

        if (GovernanceManager.Instance != null)
        {
            GovernanceManager.Instance.globalPlayerInfluence = data.globalPlayerInfluence;

            GovernanceManager.Instance.npcLikability.Clear();
            for (int i = 0; i < data.govNpcNames.Count; i++)
            {
                GovernanceManager.Instance.npcLikability.Add(data.govNpcNames[i], data.govNpcLikabilities[i]);
            }

            GovernanceManager.Instance.tomorrowEvents.Clear();
            foreach (var savedEvt in data.savedTomorrowEvents)
            {
                NpcData foundNpc = GovernanceManager.Instance.allNpcDatabase.Find(x => x.npcName == savedEvt.npcName);
                if (foundNpc != null)
                {
                    QuestData foundQuest = foundNpc.possibleQuests.Find(x => x.name == savedEvt.questName);
                    if (foundQuest != null)
                    {
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

    public void DeleteSaveData(int slotNumber)
    {
        // 💡 [수정됨] 이름 틀리지 않게, 위에서 쓰던 안전한 경로 생성기(GetSaveFilePath)를 재활용합니다!
        string path = GetSaveFilePath(slotNumber);

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"💥 {slotNumber}번 세이브 파일이 완벽하게 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning("🤔 지울 세이브 파일이 애초에 없습니다!");
        }
    }

    // ==========================================
    // 💡 [추가] 세이브 파일 겉표지(미리보기)만 살짝 읽어오는 함수
    // ==========================================
    public SaveData GetSaveDataPreview(int slotNumber)
    {
        string path = GetSaveFilePath(slotNumber);
        if (!File.Exists(path)) return null; // 파일 없으면 null 반환

        // 파일 내용을 텍스트로 읽어서 데이터 박스로 조립한 뒤 전달만 해줍니다. (적용은 안 함!)
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}