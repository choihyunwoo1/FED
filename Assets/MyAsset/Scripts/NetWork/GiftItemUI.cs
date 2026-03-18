using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GiftItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public TextMeshProUGUI amountText;

    private string myItemName;
    private ItemData myItemInfo;

    // 💡 목록을 그릴 때 매니저가 데이터를 쏴주는 함수
    public void SetupItem(string itemName, int amount, ItemData itemInfo)
    {
        myItemName = itemName;
        myItemInfo = itemInfo;

        // 아이콘과 수량만 심플하게 표시!
        if (amountText != null) amountText.text = $"보유: {amount}개";

        if (itemInfo != null && itemIcon != null && itemInfo.itemIcon != null)
        {
            itemIcon.sprite = itemInfo.itemIcon;
        }
    }

    // 💡 [선물하기] 버튼을 누르면 실행될 함수
    public void OnClickGiveButton()
    {
        // 💡 팝업을 띄우고, 괄호 { } 안의 코드를 "확인 눌렀을 때 실행해!" 하고 던져줍니다.
        PopConfirm.Instance.ShowPopup($"[{myItemName}]을(를) 선물하시겠습니까?", () =>
        {
            DateUI dateUI = FindAnyObjectByType<DateUI>();
            if (dateUI != null) dateUI.GiveGift(myItemName);

            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        });
    }

    // 🖱️ [마우스가 아이템 위로 올라왔을 때] 툴팁 켜기
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && myItemInfo != null)
        {
            // 선물 창이니까 '호감도' 정보만 깔끔하게 조립해서 던져줍니다!
            string details = $"호감도: <color=#FF69B4>+{myItemInfo.affinityBonus}</color>";
            TooltipManager.Instance.ShowTooltip(myItemName, details);
        }
    }

    // 🖱️ [마우스가 아이템 밖으로 나갔을 때] 툴팁 끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}