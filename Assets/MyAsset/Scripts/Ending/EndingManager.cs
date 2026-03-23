using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum EndingType
{
    Bankruptcy,         // 파산 엔딩 (빚쟁이)
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
    // 💡 bankruptcyMoneyLimit은 이제 안 씁니다! (무조건 0원 미만이면 파산)
    public long matrixMoneyLimit = 10000000000;     // 매트릭스 컷 (100억)
    public long crossedLineLimit = 1000000000000;   // 선 넘음 컷 (1조)
    public int majorShareholderAmount = 10000000;   // 대주주 요구 주식 수 (1000만 주)
    public int eliteInfluenceLimit = 300;           // 엘리트/퍼펫 요구 영향력

    public bool isGameEnded = false;

    [Header("엔딩용 사진 설정 (에디터에서 넣어주세요)")]
    public Sprite bankruptcyImg;
    public Sprite haremImg;
    public Sprite majorShareholderImg;
    public Sprite AscensionImg;
    public Sprite CrossedTheLineImg;
    public Sprite EliteImg;
    public Sprite PuppetImg;
    public Sprite MatrixImg;

    private void Awake() { Instance = this; }

    // ==================================================
    // ⚡ 1. [즉시 발동 엔딩] 매일, 또는 주식을 살 때마다 체크!
    // ==================================================
    public void CheckInstantEndings()
    {
        if (isGameEnded) return;

        // 🚨 즉시 파산(돈 다 잃으면 게임오버) 조건은 기획대로 완전히 삭제했습니다!

        // 1-1. 대주주 엔딩 (아무 종목이나 천만 주 달성)
        foreach (var stock in PlayerManager.Instance.portfolio.Values)
        {
            if (stock.amount >= majorShareholderAmount)
            {
                TriggerEnding(EndingType.MajorShareholder);
                return;
            }
        }

        // 1-2. 하렘 엔딩 (모든 히로인 호감도 MAX 확인)
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

        long finalMoney = PlayerManager.Instance.money; // 대출 시스템 생기면 여기서 대출금 빼기!
        int finalInfluence = GovernanceManager.Instance.globalPlayerInfluence;

        // 우선순위 0: 파산 엔딩 (잔고가 마이너스면 얄짤없이 파산!)
        if (finalMoney < 0)
        {
            TriggerEnding(EndingType.Bankruptcy);
        }
        // 우선순위 1: 승천 엔딩 (성경책 보유 + 전 재산 0원)
        else if (PlayerManager.Instance.HasItem("성경책") && finalMoney == 0)
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
        // 우선순위 5: 매트릭스 평민 (0원 이상 ~ 100억 미만)
        else
        {
            TriggerEnding(EndingType.Matrix);
        }
    }

    // ==================================================
    // 🎬 3. 엔딩 연출 및 씬 이동 로직
    // ==================================================
    // 💡 [중도 포기] 버튼을 눌렀을 때 밖에서 직접 파산 엔딩을 호출할 수 있게 public으로 열어둡니다!
    public void TriggerEnding(EndingType type)
    {
        isGameEnded = true;

        if (DayManager.Instance != null) DayManager.Instance.isTimeFlowing = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
            UIManager.Instance.OpenPanel("PopEventCutScene");

            EventCutSceneUI popup = FindAnyObjectByType<EventCutSceneUI>();
            if (popup != null)
            {
                string endingTitle = "";
                string endingDesc = "";
                Sprite endingSprite = null;

                // 💡 유저님이 에디터 인스펙터에 넣은 사진들이 실제 화면에 뜨도록 endingSprite = 변수명; 으로 다 연결해 드렸습니다!
                switch (type)
                {
                    case EndingType.Bankruptcy:
                        endingTitle = "<color=red>[ 파 산 ]</color>";
                        endingDesc = "모든 것을 잃었습니다. 차가운 길바닥이 당신을 반깁니다.\n당신의 투자는 실패로 끝났습니다.";
                        endingSprite = bankruptcyImg;
                        break;

                    case EndingType.Harem:
                        endingTitle = "<color=pink>[ 하 렘 ]</color>";
                        endingDesc = "당신은 4명의 히로인들과 행복한 나날을 보냅니다.\n이보다 더 완벽한 삶이 있을까요?";
                        endingSprite = haremImg;
                        break;

                    case EndingType.MajorShareholder:
                        endingTitle = "<color=yellow>[ 대 주 주 ]</color>";
                        endingDesc = "한 종목의 주식을 완전히 장악했습니다.\n이제 이 회사는 당신의 것입니다.";
                        endingSprite = majorShareholderImg;
                        break;

                    case EndingType.Ascension:
                        endingTitle = "<color=white>[ 승 천 ]</color>";
                        endingDesc = "물질에 얽매이지 않는 진정한 자유를 얻었습니다.\n하늘문이 열립니다.";
                        endingSprite = AscensionImg;
                        break;

                    case EndingType.CrossedTheLine:
                        endingTitle = "<color=purple>[ 선을 넘었다 ]</color>";
                        endingDesc = "한 개인의 자산이 누군가들을 자극했습니다.\n긴급 화폐개혁이 단행되어 당신의 돈은 휴지조각이 되었습니다.";
                        endingSprite = CrossedTheLineImg;
                        break;

                    case EndingType.Elite:
                        endingTitle = "<color=orange>[ NWO ]</color>";
                        endingDesc = "당신도 또한 그들처럼 사탄에게 영혼을 팔았습니다....\n사이코패스가 되어버렸네요~.";
                        endingSprite = EliteImg;
                        break;

                    case EndingType.Puppet:
                        endingTitle = "<color=brown>[ 퍼펫 ]</color>";
                        endingDesc = "평생 그 종이 쪼가리에 묶여서 살겠군요!.\n어차피 죽으면 못 가지고 갈텐데요~ㅎㅎ.";
                        endingSprite = PuppetImg;
                        break;

                    case EndingType.Matrix:
                        endingTitle = "<color=black>[ 매트릭스에 갇혔다. ]</color>";
                        endingDesc = "이곳이 게임속이라는 건 알까요?.\n사실 여러분들과 별 다를바는 없지만요^^.";
                        endingSprite = MatrixImg;
                        break;
                }

                popup.SetupStory(endingSprite, $"{endingTitle}\n\n{endingDesc}");
            }
        }

        Debug.Log($"🎉 [엔딩 도달] 플레이어가 {type} 엔딩을 보았습니다!");
    }
}