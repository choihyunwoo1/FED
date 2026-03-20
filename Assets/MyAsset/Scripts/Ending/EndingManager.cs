using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요!
using System.Collections.Generic;

public enum EndingType
{
    Bankruptcy,         // 파산 엔딩
    Harem,              // 하렘 엔딩
    MajorShareholder,   // 대주주 엔딩
    Ascension,          // 승천 (진) 엔딩
    CrossedTheLine,     // 선을 넘었다 엔딩 (1조)
    Elite,              // 엘리트 엔딩
    Puppet,             // 퍼펫 엔딩
    Matrix              // 매트릭스 평민 엔딩
}

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance;

    [Header("엔딩 밸런스 설정 (인스펙터에서 조절)")]
    public int finalDay = 300;                      // 게임 종료 일자
    public long bankruptcyMoneyLimit = 1000000;     // 마지막 날 기준 파산 컷 (1백만)
    public long matrixMoneyLimit = 10000000000;     // 매트릭스 컷 (100억)
    public long crossedLineLimit = 1000000000000;   // 선 넘음 컷 (1조)
    public int majorShareholderAmount = 10000000;   // 대주주 요구 주식 수 (1000만 주)
    public int eliteInfluenceLimit = 300;           // 엘리트/퍼펫 요구 영향력

    public bool isGameEnded = false;

    [Header("엔딩용 사진 설정 (에디터에서 넣어주세요)")]
    public Sprite bankruptcyImg;     // 파산 사진
    public Sprite haremImg;           // 하렘 사진
    public Sprite majorShareholderImg; // 대주주 사진
    public Sprite AscensionImg;           // 승천 (진) 엔딩
    public Sprite CrossedTheLineImg;           // 선을 넘었다 엔딩 (1조)
    public Sprite EliteImg;           // 엘리트 엔딩
    public Sprite PuppetImg;           // 퍼펫 엔딩
    public Sprite MatrixImg;           // 매트릭스 평민 엔딩

    private void Awake() { Instance = this; }

    // ==================================================
    // ⚡ 1. [즉시 발동 엔딩] 매일, 또는 주식을 살 때마다 체크!
    // ==================================================
    public void CheckInstantEndings()
    {
        if (isGameEnded) return;

        // 1-1. 파산 엔딩 (현금 3만원 이하 & 주식 0주)
        // (기존 PlayerManager.CheckGameOver 역할을 얘가 대신합니다)
        if (PlayerManager.Instance.money < 30000 && PlayerManager.Instance.portfolio.Count == 0)
        {
            TriggerEnding(EndingType.Bankruptcy);
            return;
        }

        // 1-2. 대주주 엔딩 (아무 종목이나 천만 주 달성)
        foreach (var stock in PlayerManager.Instance.portfolio.Values)
        {
            if (stock.amount >= majorShareholderAmount)
            {
                TriggerEnding(EndingType.MajorShareholder);
                return;
            }
        }

        // 1-3. 하렘 엔딩 (모든 히로인 호감도 MAX 확인)
        if (DateManager.Instance != null && DateManager.Instance.npcList.Count > 0)
        {
            bool isHarem = true;
            foreach (var npc in DateManager.Instance.npcList)
            {
                if (npc.currentAffinity < npc.maxAffinity)
                {
                    isHarem = false;
                    break;
                }
            }
            if (isHarem)
            {
                TriggerEnding(EndingType.Harem);
                return;
            }
        }
    }

    // ==================================================
    // 🌅 2. [마지막 날 발동 엔딩] Day 300 아침에만 체크!
    // ==================================================
    public void CheckLastDayEndings()
    {
        if (isGameEnded) return;

        long finalMoney = PlayerManager.Instance.money;
        int finalInfluence = GovernanceManager.Instance.globalPlayerInfluence;

        // ⚠️ 조건이 겹칠 수 있으므로 '우선순위(Priority)'가 가장 높은 것부터 if문으로 거릅니다!

        // 우선순위 1: 승천 엔딩 (성경책 보유 + 전 재산 0원)
        if (PlayerManager.Instance.HasItem("성경책") && finalMoney == 0)
        {
            TriggerEnding(EndingType.Ascension);
        }
        // 우선순위 2: 선을 넘었다 (1조 이상)
        else if (finalMoney >= crossedLineLimit)
        {
            TriggerEnding(EndingType.CrossedTheLine);
        }
        // 우선순위 3: 엘리트 (100억 이상 & 영향력 300 이상)
        else if (finalMoney >= matrixMoneyLimit && finalInfluence >= eliteInfluenceLimit)
        {
            TriggerEnding(EndingType.Elite);
        }
        // 우선순위 4: 퍼펫 (영향력은 높은데 돈이 100억 미만일 때)
        else if (finalInfluence >= eliteInfluenceLimit)
        {
            TriggerEnding(EndingType.Puppet);
        }
        // 우선순위 5: 매트릭스 평민 (1백만 ~ 100억 사이)
        else if (finalMoney >= bankruptcyMoneyLimit)
        {
            TriggerEnding(EndingType.Matrix);
        }
        // 우선순위 6: 파산 (나머지, 1백만원 미만)
        else
        {
            TriggerEnding(EndingType.Bankruptcy);
        }
    }

    // ==================================================
    // 🎬 3. 엔딩 연출 및 씬 이동 로직
    // ==================================================
    private void TriggerEnding(EndingType type)
    {
        isGameEnded = true;

        if (DayManager.Instance != null) DayManager.Instance.isTimeFlowing = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
            UIManager.Instance.OpenPanel("PopEventCutScene"); // 만능 팝업창 호출!

            EventCutSceneUI popup = FindAnyObjectByType<EventCutSceneUI>();
            if (popup != null)
            {
                string endingTitle = "";
                string endingDesc = "";
                Sprite endingSprite = null; // 던져줄 사진 변수

                // 💡 [여기입니다!] 여기서 8개 엔딩의 내용을 다 채워 넣으세요!
                switch (type)
                {
                    case EndingType.Bankruptcy:
                        endingTitle = "<color=red>[ 파 산 ]</color>";
                        endingDesc = "모든 것을 잃었습니다. 차가운 길바닥이 당신을 반깁니다.\n당신의 투자는 실패로 끝났습니다.";
                        endingSprite = null; // 에디터에서 넣은 사진 연결
                        break;

                    case EndingType.Harem:
                        endingTitle = "<color=pink>[ 하 렘 ]</color>";
                        endingDesc = "당신은 4명의 히로인들과 행복한 나날을 보냅니다.\n이보다 더 완벽한 삶이 있을까요?";
                        endingSprite = null;
                        break;

                    case EndingType.MajorShareholder:
                        endingTitle = "<color=yellow>[ 대 주 주 ]</color>";
                        endingDesc = "한 종목의 주식을 완전히 장악했습니다.\n이제 이 회사는 당신의 것입니다.";
                        endingSprite = null;
                        break;

                    case EndingType.Ascension:
                        endingTitle = "<color=white>[ 승 천 ]</color>";
                        endingDesc = "물질에 얽매이지 않는 진정한 자유를 얻었습니다.\n하늘문이 열립니다.";
                        endingSprite = null; 
                        break;

                    case EndingType.CrossedTheLine:
                        endingTitle = "<color=purple>[ 선을 넘었다 ]</color>";
                        endingDesc = "한 개인의 자산이 누군가들을 자극했습니다.\n긴급 화폐개혁이 단행되어 당신의 돈은 휴지조각이 되었습니다.";
                        endingSprite = null;
                        break;

                    case EndingType.Elite:
                        endingTitle = "<color=orange>[ NWO ]</color>";
                        endingDesc = "당신도 또한 그들처럼 사탄에게 영혼을 팔았습니다....\n사이코패스가 되어버렸네요~.";
                        endingSprite = null;
                        break;

                    case EndingType.Puppet:
                        endingTitle = "<color=brown>[ 퍼펫 ]</color>";
                        endingDesc = "평생 그 종이 쪼가리에 묶여서 살겠군요!.\n어차피 죽으면 못 가지고 갈텐데요~ㅎㅎ.";
                        endingSprite = null;
                        break;

                    case EndingType.Matrix:
                        endingTitle = "<color=black>[ 매트릭스에 갇혔다. ]</color>";
                        endingDesc = "이곳이 게임속이라는 건 알까요?.\n사실 여러분들과 별 다를바는 없지만요^^.";
                        endingSprite = null;
                        break;
                }

                // 💡 팝업창에 데이터 던지기!
                popup.SetupStory(endingSprite, $"{endingTitle}\n\n{endingDesc}");
            }
        }

        Debug.Log($"🎉 [엔딩 도달] 플레이어가 {type} 엔딩을 보았습니다!");
    }
}