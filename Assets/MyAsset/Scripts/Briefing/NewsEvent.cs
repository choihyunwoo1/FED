using UnityEngine;

// 💡 뉴스의 종류를 나누는 명찰!
public enum NewsType
{
    Headline, // 대서특필 (왼쪽 큰 뉴스)
    Rumor     // 찌라시 (오른쪽 작은 뉴스)
}

[System.Serializable]
public class NewsEvent
{
    public string headline;      // 뉴스 헤드라인 (예: "A전자, 세기의 M&A 체결!")
    [TextArea(2, 3)]
    public string description;   // 상세 내용 (예: "경쟁사를 인수하며 시장 점유율 1위 달성...")

    // 💡 추가된 부분: 뉴스의 종류와 사진!
    [Header("UI Settings")]
    public NewsType type = NewsType.Rumor; // 기본값은 찌라시로 설정
    public Sprite newsImage;               // 대서특필에 들어갈 사진 (없으면 비워둬도 됨)

    // 💡 이 뉴스가 누구에게 타격을 줄 것인가?
    // 0, 1, 2... = 특정 주식 인덱스 (A전자, B건설 등)
    // -1 = 거시 경제 (시장 전체 주식에 동시 타격! 예: FOMC 금리 인상)
    public int targetStockIndex = -1;

    [Header("Impact Settings (영향력)")]
    // 💡 주가에 미치는 즉각적인 타격 (예: 0.3f = 즉시 +30% 상승, -0.5f = 즉시 반토막)
    public float priceChangePercent;

    // 💡 이후 차트의 흔들림(변동성) 변화 (예: 0.1f = 이 뉴스 이후로 차트가 미친듯이 요동침)
    public float volatilityChange;

    [Header("Trigger Settings")]
    public bool canHappenRandomly = true; // 체크 해제(false)하면 절대 랜덤으로 안 터짐! (대형주 전용)

    // 💡 (이 뉴스는 한 번 터지면 끝인가요?)
    public bool isOneTimeOnly = true;

    // 💡 시스템이 기억하기 위한 내부 변수 (인스펙터엔 안 보임)
    [HideInInspector]
    public bool hasBeenPublished = false;
}