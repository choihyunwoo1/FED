using System.Collections.Generic;
using UnityEngine;

// 💡 [System.Serializable]을 꼭 붙여야 유니티가 이 상자를 JSON 텍스트로 압축할 수 있습니다!
[System.Serializable]
public class SaveData
{
    [Header("플레이어 자산")]
    public long money;

    [Header("인벤토리 (잡동사니)")]
    public List<string> inventoryItemNames = new List<string>();
    public List<int> inventoryItemAmounts = new List<int>();

    [Header("포트폴리오 (주식)")]
    public List<string> portfolioStockNames = new List<string>();
    public List<int> portfolioStockAmounts = new List<int>();
    public List<int> portfolioStockAvgPrices = new List<int>();

    [Header("상점 & NPC 상태")]
    public int shopLevel;
    public List<int> npcAffinities = new List<int>();
    public List<bool> npcRewardsGiven = new List<bool>();

    [Header("가계부 데이터")]
    public long weeklyEarned;
    public long weeklySpent;
    public List<WeeklyRecord> weeklyHistory = new List<WeeklyRecord>();

    [Header("거버넌스 (로비/정치)")]
    public int globalPlayerInfluence;
    public List<string> govNpcNames = new List<string>();
    public List<int> govNpcLikabilities = new List<int>();

    // 💡 SO(스크립터블 오브젝트)는 직접 저장이 안 되므로, 이름만 저장할 특수 구조체!
    [System.Serializable]
    public class SavedPendingEvent
    {
        public string npcName;
        public string questName; // 퀘스트 파일의 이름
        public bool isSuccess;
    }
    public List<SavedPendingEvent> savedTomorrowEvents = new List<SavedPendingEvent>();

    // TODO: 나중에 DayManager의 현재 날짜(Day) 변수도 여기에 추가하면 완벽합니다!
}