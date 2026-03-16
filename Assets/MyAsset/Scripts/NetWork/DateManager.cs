using System.Collections.Generic;
using UnityEngine;

// 💡 NPC 1명의 모든 정보를 담는 데이터 박스
[System.Serializable]
public class NPCData
{
    public string npcName;           // 이름 (예: "비서실장 최지윤")
    public Sprite portrait;          // 캐릭터 스탠딩 이미지

    [Header("호감도 시스템")]
    public int currentAffinity = 0;  // 현재 호감도
    public int maxAffinity = 100;    // 최대 호감도
    public string favoriteGiftName = "고급 와인";
    public int giftAffinityUp = 10;  // 선물 한 번에 오르는 호감도 수치

    [Header("대화 시스템")]
    [TextArea(2, 3)]
    public List<string> normalDialogues; // 일상 대화들 (랜덤 출력)

    [Header("비밀 정보 보상")]
    public int rewardThreshold = 50; // 호감도 몇 달성 시 정보를 줄 것인가?
    public string secretInfoText;    // 달성 시 팝업으로 띄워줄 특급 찌라시!
    public bool hasGivenSecret = false; // 보상을 이미 줬는지 체크 (중복 수령 방지)
}

public class DateManager : MonoBehaviour
{
    public static DateManager Instance;

    public List<NPCData> npcList = new List<NPCData>();

    // 현재 데이트 중인 NPC의 인덱스
    public int currentNpcIndex = 0;

    private void Awake() { Instance = this; }

    // 현재 만나고 있는 NPC 데이터 가져오기
    public NPCData GetCurrentNPC()
    {
        if (npcList.Count > currentNpcIndex)
            return npcList[currentNpcIndex];
        return null;
    }
}