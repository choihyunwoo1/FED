using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 💡 [핵심] 마우스 감지 기능을 쓰기 위해 꼭 추가해야 합니다!

// 💡 클래스 이름 옆에 IPointerEnterHandler, IPointerExitHandler를 추가합니다!
public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public TextMeshProUGUI amountText; // 💡 이름, 가격 텍스트는 이제 필요 없으니 지우셔도 됩니다!

    private string myItemName; //이름
    private int mySellPrice; //판매 가격

    private ItemData myItemInfo;
    public void SetupItem(string itemName, int amount, ItemData itemInfo)
    {
        myItemName = itemName;
        myItemInfo = itemInfo; // 나중에 툴팁에 띄우기 위해 정보 박스를 기억해둡니다.

        if (amountText != null) amountText.text = amount.ToString(); // "99" 처럼 숫자만 표시
        if (itemInfo != null && itemIcon != null && itemInfo.itemIcon != null)
            itemIcon.sprite = itemInfo.itemIcon;

        mySellPrice = itemInfo != null ? itemInfo.price / 2 : 0;
    }

    public void OnClickSellButton()
    {
        // 💡 내 가방에 있는 현재 개수를 최대치로 설정
        int myAmount = PlayerManager.Instance.inventory[myItemName];

        PopAmount.Instance.ShowPopup($"[{myItemName}] 판매", mySellPrice, myAmount, (amount) =>
        {
            PlayerManager.Instance.RemoveItem(myItemName, amount);
            PlayerManager.Instance.AddMoney(mySellPrice * amount);

            PopInventoryUI invUI = FindAnyObjectByType<PopInventoryUI>();
            if (invUI != null) invUI.RefreshInventory();

            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        });
    }

    // 🖱️ [마우스가 아이템 UI 위로 들어왔을 때 자동 실행됨]
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && myItemInfo != null)
        {
            // 툴팁에 띄울 설명을 예쁘게 조립합니다.
            string details = $"팔기: <color=yellow>{mySellPrice:N0}원</color>\n호감도: <color=#FF69B4>+{myItemInfo.affinityBonus}</color>";

            // 매니저에게 띄워달라고 요청!
            TooltipManager.Instance.ShowTooltip(myItemName, details);
        }
    }

    // 🖱️ [마우스가 아이템 UI 밖으로 나갔을 때 자동 실행됨]
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}