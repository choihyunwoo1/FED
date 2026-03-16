using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GiftItemUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI affinityText; // "호감도 +5" 표시용

    private string myItemName;

    // 💡 목록을 그릴 때 매니저가 데이터를 쏴주는 함수
    public void SetupItem(string itemName, int amount, ItemData itemInfo)
    {
        myItemName = itemName;

        if (amountText != null) amountText.text = $"보유: {amount}개";

        if (itemInfo != null)
        {
            if (itemIcon != null && itemInfo.itemIcon != null)
                itemIcon.sprite = itemInfo.itemIcon;

            if (affinityText != null)
                affinityText.text = $"♥ +{itemInfo.affinityBonus}";
        }
    }

    // 💡 [선물하기] 버튼을 누르면 실행될 함수
    public void OnClickGiveButton()
    {
        DateUI dateUI = FindAnyObjectByType<DateUI>();

        if (dateUI != null)
        {
            // DateUI에게 "이 아이템 줘!" 라고 전달만 하면 끝!
            // (서랍장 닫는 건 위의 GiveGift 함수가 알아서 해줍니다)
            dateUI.GiveGift(myItemName);
        }
    }
}