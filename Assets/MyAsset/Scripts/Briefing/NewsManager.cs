using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections; // 💡 코루틴을 쓰기 위해 추가!

public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance;

    [Header("News Database")]
    public List<NewsEvent> newsDatabase = new List<NewsEvent>();

    [Header("Published News")]
    public List<NewsEvent> publishedNews = new List<NewsEvent>();

    // 💡 현재 장이 열려있는가? (다른 스크립트에서 true/false로 조종하면 됩니다!)
    [Header("Market State")]
    public bool isMarketOpen = true;

    // 💡 찌라시 자동 살포기 세팅!
    [Header("Auto Random News Settings")]
    public bool isAutoNewsActive = true;    // 자동 뉴스 켜기/끄기
    public float randomIntervalMin = 15f;   // 최소 15초마다
    public float randomIntervalMax = 40f;   // 최대 40초마다 찌라시 터짐!

    public event Action OnNewsPublished;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupInitialNews();

        // 💡 게임 시작과 동시에 랜덤 찌라시 살포기 가동!
        StartCoroutine(AutoRandomNewsRoutine());
    }

    private void SetupInitialNews()
    {
        // 1. 대형주 호재 (A전자) ➔ 랜덤 찌라시 금지! (canHappenRandomly = false)
        NewsEvent news1 = new NewsEvent
        {
            headline = "[특보] A전자, 세기의 M&A 체결!",
            description = "글로벌 시장 점유율 1위 달성 전망...",
            targetStockIndex = 0,
            priceChangePercent = 0.3f,
            volatilityChange = 0.05f,
            canHappenRandomly = false, // 🚨 대형주 뉴스는 유저가 원할 때나 정해진 시간에만 뜹니다!
            isOneTimeOnly = true,
            type = NewsType.Headline
        };

        // 2. 바이오/잡주 찌라시 (C바이오) ➔ 수시로 터짐 (canHappenRandomly = true)
        NewsEvent news2 = new NewsEvent
        {
            headline = "[루머] C바이오, 신약 임상 3상 실패설 돌며 투심 악화",
            description = "회사 측은 사실무근이라며 진화에 나섰으나...",
            targetStockIndex = 2, // C바이오
            priceChangePercent = -0.15f,
            volatilityChange = 0.15f,
            canHappenRandomly = true,  // 🎲 이건 랜덤 찌라시로 수시로 튀어나옴!
            isOneTimeOnly = false,
            type = NewsType.Rumor
        };

        newsDatabase.Add(news1);
        newsDatabase.Add(news2);
    }

    // 🎲 무작위 시간에 찌라시를 터뜨리는 자동화 코루틴
    private IEnumerator AutoRandomNewsRoutine()
    {
        while (isAutoNewsActive)
        {
            if (isMarketOpen)
            {
                // 1. 목표 대기 시간을 정합니다 (예: 20초)
                float targetWaitTime = UnityEngine.Random.Range(randomIntervalMin, randomIntervalMax);
                float currentTimer = 0f;

                // 2. 타이머가 목표 시간에 도달할 때까지 돕니다.
                while (currentTimer < targetWaitTime)
                {
                    // 💡 핵심: 장이 열려있을 때만 타이머가 흘러갑니다!
                    // (만약 10초 지났는데 장이 닫히면, 타이머가 일시정지됩니다)
                    if (isMarketOpen)
                    {
                        currentTimer += Time.deltaTime;
                    }

                    yield return null; // 1프레임 대기
                }

                // 3. 타이머가 꽉 차면 랜덤 뉴스 발사!
                TriggerRandomNews();
            }
            else
            {
                // 장이 닫혀있으면 타이머를 돌리지 않고 그냥 대기합니다.
                yield return null;
            }
        }
    }

    // 🎯 랜덤 찌라시 발사기 (필터링 강화)
    public void TriggerRandomNews()
    {
        List<int> validIndices = new List<int>();
        for (int i = 0; i < newsDatabase.Count; i++)
        {
            NewsEvent news = newsDatabase[i];

            // 💡 조건: 랜덤 허용인가? AND (다회용이거나 OR 아직 한 번도 안 터졌거나)
            if (news.canHappenRandomly)
            {
                if (!news.isOneTimeOnly || !news.hasBeenPublished)
                {
                    validIndices.Add(i); // 합격! 발사 후보에 등록
                }
            }
        }

        if (validIndices.Count == 0) return; // 쏠 뉴스가 다 고갈됐으면 패스

        int randomIndex = validIndices[UnityEngine.Random.Range(0, validIndices.Count)];
        TriggerNews(randomIndex);
    }

    // 🚨 수동/확정 발사기 (방어막 추가)
    public void TriggerNews(int newsIndex)
    {
        if (newsIndex < 0 || newsIndex >= newsDatabase.Count) return;

        NewsEvent triggeredNews = newsDatabase[newsIndex];

        // 💡 방어막: 1회용 뉴스인데 이미 터진 적이 있다면? 조용히 취소!
        if (triggeredNews.isOneTimeOnly && triggeredNews.hasBeenPublished)
        {
            Debug.Log($"⚠️ [{triggeredNews.headline}] 뉴스는 이미 방송되어 무시됩니다.");
            return;
        }

        // "나 방금 방송 탔다!" 하고 도장 쾅 찍어주기
        triggeredNews.hasBeenPublished = true;

        // (아래는 기존 코드와 동일)
        publishedNews.Insert(0, triggeredNews);
        ApplyNewsImpact(triggeredNews);
        OnNewsPublished?.Invoke();

        Debug.Log($"📰 [뉴스 터짐] {triggeredNews.headline}");
    }

    private void ApplyNewsImpact(NewsEvent news)
    {
        if (StockManager.Instance == null) return;

        if (news.targetStockIndex >= 0 && news.targetStockIndex < StockManager.Instance.stockList.Count)
        {
            ApplyImpactToStock(StockManager.Instance.stockList[news.targetStockIndex], news);
        }
        else if (news.targetStockIndex == -1)
        {
            foreach (Stock stock in StockManager.Instance.stockList) ApplyImpactToStock(stock, news);
        }
        StockManager.Instance.UpdateAllStocks();
    }

    private void ApplyImpactToStock(Stock stock, NewsEvent news)
    {
        stock.currentPrice = Mathf.RoundToInt(stock.currentPrice * (1f + news.priceChangePercent));
        stock.volatility += news.volatilityChange;
        if (stock.currentPrice < 10) stock.currentPrice = 10;
    }
}