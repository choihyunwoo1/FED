// NpcData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewNpc", menuName = "Governance/NpcData")]
public class NpcData : ScriptableObject
{
    public string npcName;
    public Sprite npcImage;
    public FactionType faction;

    // 이 NPC가 줄 수 있는 퀘스트 풀 (랜덤으로 나옴)
    public List<QuestData> possibleQuests = new List<QuestData>();
}