using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField] private TextMeshProUGUI assetText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

    // 💡 새로 추가! (통제할 버튼 3인방)
    [Header("Time Control Buttons")]
    public GameObject startTradingBtn; // Start 버튼
    public GameObject endTradingBtn;   // End 버튼
    public GameObject nextDayBtn;      // Next 버튼

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnMoneyChanged += UpdateAssetUI;
            UpdateAssetUI(PlayerManager.Instance.money);
        }

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnTimeChanged += UpdateTimeUI;
            UpdateTimeUI();
        }
    }

    public void UpdateAssetUI(long currentMoney)
    {
        assetText.text = currentMoney.ToString("N0");
    }

    public void UpdateTimeUI()
    {
        if (DayManager.Instance == null) return;

        int day = DayManager.Instance.currentDay;
        int hour = DayManager.Instance.currentHour;
        int min = DayManager.Instance.currentMinute;
        DayPhase phase = DayManager.Instance.currentPhase;

        dayText.text = "Day " + day.ToString();

        // 💡 페이즈에 따라 글씨와 버튼을 동시에 통제합니다!
        if (phase == DayPhase.Morning)
        {
            timeText.text = "아침 (개장 전)";

            // 아침엔 '장 시작(Start)' 버튼만 켬!
            if (startTradingBtn != null) startTradingBtn.SetActive(true);
            if (endTradingBtn != null) endTradingBtn.SetActive(false);
            if (nextDayBtn != null) nextDayBtn.SetActive(false);
        }
        else if (phase == DayPhase.Trading)
        {
            timeText.text = $"{hour:D2}:{min:D2}";

            // 낮에는 '장 마감(End)' 버튼만 켬!
            if (startTradingBtn != null) startTradingBtn.SetActive(false);
            if (endTradingBtn != null) endTradingBtn.SetActive(true);
            if (nextDayBtn != null) nextDayBtn.SetActive(false);
        }
        else if (phase == DayPhase.Evening)
        {
            timeText.text = "오후 (장 마감)";

            // 밤에는 '다음 날(Next)' 버튼만 켬!
            if (startTradingBtn != null) startTradingBtn.SetActive(false);
            if (endTradingBtn != null) endTradingBtn.SetActive(false);
            if (nextDayBtn != null) nextDayBtn.SetActive(true);
        }
    }
}