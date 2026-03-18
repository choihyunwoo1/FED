using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;

    private ItemData myItemInfo; // 이 진열대가 팔고 있는 아이템의 상세 정보

    // 💡 상점 매니저가 진열대를 차릴 때 정보를 쏴주는 함수
    public void SetupItem(ItemData data)
    {
        myItemInfo = data;

        if (itemIcon != null && data.itemIcon != null)
        {
            itemIcon.sprite = data.itemIcon;
        }
    }

    // 💸 [구매하기] 버튼을 누르면 실행될 함수
    public void OnClickBuyButton()
    {
        // 💡 내가 가진 돈으로 최대 몇 개 살 수 있는지 계산 (최대 99개 제한)
        int maxBuy = (int)(PlayerManager.Instance.money / myItemInfo.price);
        if (maxBuy < 1)
        {
            Debug.LogWarning("돈이 부족합니다!");
            return;
        }
        maxBuy = Mathf.Min(maxBuy, 99);

        PopAmount.Instance.ShowPopup($"[{myItemInfo.itemName}] 구매", myItemInfo.price, maxBuy, (amount) =>
        {
            if (PlayerManager.Instance.SubtractMoney(myItemInfo.price * amount))
            {
                PlayerManager.Instance.AddItem(myItemInfo.itemName, amount);
                Debug.Log($"🛒 {myItemInfo.itemName} {amount}개 구매 완료!");
            }
        });
    }

    // 🖱️ [마우스가 아이템 위로 올라왔을 때] 툴팁 켜기
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && myItemInfo != null)
        {
            // 상점이니까 '구매가'와 '호감도' 정보를 예쁘게 조립합니다!
            string details = $"구매가: <color=yellow>{myItemInfo.price:N0}원</color>\n호감도: <color=#FF69B4>+{myItemInfo.affinityBonus}</color>";

            // 매니저에게 띄워달라고 요청! (이름도 같이 넘겨줍니다)
            TooltipManager.Instance.ShowTooltip(myItemInfo.itemName, details);
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