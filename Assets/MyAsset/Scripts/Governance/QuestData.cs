// QuestData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Governance/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 설명 (Npc가 대화창에서 할 말)")]
    [TextArea(3, 10)]
    public string questDescription; // "~에 원만큼 지원해주지 않겠나?"

    [Header("요구 사항")]
    public QuestReqType reqType;    // 돈? 주식 팔기? 영향력 쓰기?
    public int reqValue;            // 돈 액수, 혹은 영향력 소모량
    public string targetStockName;  // [주식 팔기]일 때 목표 주식 이름

    [Header("기본 성공률 (0~100)")]
    [Range(0, 100)]
    public int baseSuccessRate = 70;

    [Header("보상 설정")]
    public QuestRewardType rewardType; // 보상 종류
    public int rewardValue;           // 보상 액수/수치
    public string targetHeroineName;  // [히로인 호감도]일 때 대상 이름

    [Header("진영 알력 다툼 (이 퀘스트 수락 시)")]
    public FactionType targetFaction; // 이 퀘스트의 소속 진영
    [Range(0, 50)]
    public int otherFactionLikenessPenalty = 5; // 다른 진영 호감도 감소량
}