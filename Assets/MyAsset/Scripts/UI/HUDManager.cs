using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField] private TextMeshProUGUI assetText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

    // 🚨 [Header("Money")]와 관련된 변수(currentMoney)는 과감히 삭제!

    [Header("Time Settings")]
    private float timer = 0f;
    private float timePerTick = 1f;
    private int currentDay = 1;

    private int currentHour = 0;
    private int currentMinute = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateTimeUI(currentHour, currentMinute);
        UpdateDayUI(currentDay);

        // 💡 [핵심] PlayerManager가 켜져 있다면, 돈 변경 신호에 전광판을 연결합니다!
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnMoneyChanged += UpdateAssetUI;

            // 처음 시작할 때 현재 내 전 재산을 한 번 띄워줍니다.
            UpdateAssetUI(PlayerManager.Instance.money);
        }
    }

    private void Update()
    {
        timer = timer + Time.deltaTime;

        if (timer >= timePerTick)
        {
            timer = 0f;
            currentMinute = currentMinute + 10;
            StockManager.Instance.UpdateAllStocks();

            if (currentMinute >= 60)
            {
                currentHour = currentHour + 1;
                currentMinute = 0;
            }

            if (currentHour >= 24)
            {
                currentDay = currentDay + 1;
                currentHour = 0;
                UpdateDayUI(currentDay);
            }

            UpdateTimeUI(currentHour, currentMinute);
        }
    }

    // 💡 PlayerManager에서 돈(long)을 넘겨주므로, 파라미터 타입을 long으로 맞춥니다.
    public void UpdateAssetUI(long currentMoney)
    {
        assetText.text = currentMoney.ToString("N0");
    }

    public void UpdateDayUI(int currentDay)
    {
        dayText.text = "Day " + currentDay.ToString();
    }

    public void UpdateTimeUI(int currentHour, int currentMinute)
    {
        timeText.text = $"{currentHour:D2}:{currentMinute:D2}";
    }

    // 🚨 SpendMoney와 AddMoney 함수는 완전히 삭제! (PlayerManager가 대신 해줌)
}