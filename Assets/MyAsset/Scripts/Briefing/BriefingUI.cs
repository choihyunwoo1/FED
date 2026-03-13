using UnityEngine;

public class BriefingUI : MonoBehaviour
{
    [Header("Headline Section (왼쪽)")]
    public GameObject headlinePrefab;  // 사진 들어간 큰 프리팹
    public Transform headlineContent;  // 왼쪽 스크롤 뷰의 Content

    [Header("Rumor Section (오른쪽)")]
    public GameObject rumorPrefab;     // 얇은 줄 프리팹 (기존에 쓰던 것)
    public Transform rumorContent;     // 오른쪽 스크롤 뷰의 Content

    private void OnEnable()
    {
        if (NewsManager.Instance != null)
        {
            NewsManager.Instance.OnNewsPublished += RefreshNewsList;
            RefreshNewsList();
        }
    }

    private void OnDisable()
    {
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsPublished -= RefreshNewsList;
    }

    public void RefreshNewsList()
    {
        // 양쪽 도화지 모두 청소
        foreach (Transform child in headlineContent) Destroy(child.gameObject);
        foreach (Transform child in rumorContent) Destroy(child.gameObject);

        if (NewsManager.Instance == null) return;

        // 뉴스를 꺼내서 타입에 따라 좌/우로 나눠서 생성!
        foreach (NewsEvent news in NewsManager.Instance.publishedNews)
        {
            if (news.type == NewsType.Headline)
            {
                // 대서특필은 왼쪽(Headline)에 생성
                GameObject go = Instantiate(headlinePrefab, headlineContent);
                go.GetComponent<NewsItem>().Setup(news);
            }
            else
            {
                // 찌라시는 오른쪽(Rumor)에 생성
                GameObject go = Instantiate(rumorPrefab, rumorContent);
                go.GetComponent<NewsItem>().Setup(news);
            }
        }
    }
}