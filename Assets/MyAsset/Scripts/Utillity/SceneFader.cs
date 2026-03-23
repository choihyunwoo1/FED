using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // 💡 코루틴(시간 지연 함수)을 쓰기 위해 필수!

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("UI 연결")]
    public CanvasGroup fadeCanvasGroup; // 💡 장막 전체의 투명도를 조절할 컨트롤러
    public TextMeshProUGUI loadingText; // "Loading..." 글씨

    [Header("설정")]
    public float fadeDuration = 0.5f;   // 암전되는 데 걸리는 시간 (0.5초)

    private bool isFading = false;      // 중복 클릭 방지용 자물쇠

    private void Awake()
    {
        // 💡 씬이 넘어가도 이 장막(페이더)이 파괴되지 않게 불사신으로 만듭니다!
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // 💡 [여기 추가!] 부모에게서 탈출해서 최상단으로 이동!
            DontDestroyOnLoad(gameObject); // 내 몸통을 파괴하지 마라!
        }
        else
        {
            Destroy(gameObject); // 이미 불사신이 있으면 짝퉁은 자결함
        }
    }

    private void Start()
    {
        // 게임 시작 시 장막은 완전히 투명(0)하고, 클릭을 방해하지 않게 꺼둡니다.
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    // 🚀 다른 스크립트에서 씬을 넘길 때 호출할 메인 함수!
    // 예: SceneFader.Instance.FadeToScene("PlayScene");
    public void FadeToScene(string sceneName)
    {
        if (isFading) return; // 이미 넘어가는 중이면 무시
        StartCoroutine(FadeAndLoadRoutine(sceneName));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        isFading = true;

        // 1. 장막을 물리적으로 켜서 마우스 클릭을 막습니다 (버튼 연타 방지)
        fadeCanvasGroup.blocksRaycasts = true;

        if (loadingText != null) loadingText.text = "Loading...";

        // 2. [Fade Out] 검은 화면이 서서히 진해집니다 (투명도 0 -> 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; // 다음 프레임까지 대기
        }
        fadeCanvasGroup.alpha = 1f; // 확실하게 1로 고정

        // 3. 💡 [비동기 로딩] 뒤에서 몰래 다음 씬을 불러옵니다!
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로딩이 끝날 때까지 대기하면서 진행률(%) 텍스트 업데이트 (선택 사항)
        while (!asyncLoad.isDone)
        {
            if (loadingText != null)
            {
                // 진행률은 0 ~ 0.9까지만 나오므로 100을 곱해서 퍼센트로 만듦
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                loadingText.text = $"Loading... {progress * 100:0}%";
            }
            yield return null;
        }

        // 4. 로딩 완료! (새로운 씬이 뒤에 깔림)

        // 5. [Fade In] 검은 화면이 서서히 걷힙니다 (투명도 1 -> 0)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // 6. 물리적 장막을 걷어서 다시 유저가 게임을 조작할 수 있게 해줍니다.
        fadeCanvasGroup.blocksRaycasts = false;
        isFading = false;
    }
}