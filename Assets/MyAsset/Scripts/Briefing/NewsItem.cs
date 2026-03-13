using UnityEngine;
using TMPro;
using UnityEngine.UI; // 💡 Image 컴포넌트를 쓰기 위해 반드시 추가!

public class NewsItem : MonoBehaviour
{
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI descriptionText;
    public Image newsImage; // 💡 사진이 들어갈 자리 (찌라시 프리팹에는 연결 안 해도 됨)

    public void Setup(NewsEvent news)
    {
        headlineText.text = news.headline;

        // 내용 텍스트가 프리팹에 있다면 연결
        if (descriptionText != null)
            descriptionText.text = news.description;

        // 💡 사진 세팅 로직
        if (newsImage != null)
        {
            if (news.newsImage != null)
            {
                newsImage.sprite = news.newsImage;
                newsImage.gameObject.SetActive(true); // 사진 켜기
            }
            else
            {
                newsImage.gameObject.SetActive(false); // 사진 데이터가 없으면 공간 숨기기
            }
        }

        // 색상 세팅 (기존과 동일)
        if (news.priceChangePercent > 0) headlineText.color = Color.red;
        else if (news.priceChangePercent < 0) headlineText.color = Color.blue;
        else headlineText.color = Color.black;
    }
}