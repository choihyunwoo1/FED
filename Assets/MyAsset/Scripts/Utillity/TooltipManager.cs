using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;

    private RectTransform canvasRect;  // 💡 캔버스 크기 정보
    private RectTransform tooltipRect; // 💡 툴팁 패널 크기 정보

    private void Awake()
    {
        Instance = this;

        // 💡 시작할 때 내 부모인 캔버스와 툴팁 패널의 뼈대를 찾아둡니다.
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();

        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 💡 [절대 마법 코드] 모니터 픽셀 좌표를 UI 캔버스 전용 좌표로 안전하게 변환합니다!
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                GetComponentInParent<Canvas>().worldCamera,
                out Vector2 localPoint);

            // 변환된 좌표에 마우스에 안 가려지게 살짝 우측 하단(+20, -20) 오프셋을 더해서 이동시킵니다.
            tooltipRect.localPosition = localPoint + new Vector2(20f, -20f);
        }
    }

    public void ShowTooltip(string itemName, string infoDetails)
    {
        tooltipPanel.SetActive(true);
        if (nameText != null) nameText.text = itemName;
        if (infoText != null) infoText.text = infoDetails;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}