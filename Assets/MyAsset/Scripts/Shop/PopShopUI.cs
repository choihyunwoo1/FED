using UnityEngine;

public class PopShopUI : MonoBehaviour
{
    public GameObject shopItemPrefab;  // 방금 만든 진열대 1칸 프리팹
    public Transform contentTransform; // Scroll View의 Content

    private void OnEnable()
    {
        RefreshShop();
    }

    public void RefreshShop()
    {
        foreach (Transform child in contentTransform) Destroy(child.gameObject);
        if (PlayerManager.Instance == null) return;

        // 💡 [핵심 1] 사장님(부모 오브젝트에 있는 ShopUI)에게 현재 상점 레벨이 몇인지 물어봅니다!
        int currentLevel = 1; // 기본 레벨
        ShopUI boss = GetComponentInParent<ShopUI>();
        if (boss != null)
        {
            currentLevel = boss.currentShopLevel;
        }

        foreach (ItemData item in PlayerManager.Instance.itemDatabase)
        {
            if (item.price <= 0) continue;

            // 💡 [핵심 2] 우리 상점 레벨이, 이 아이템의 해금 레벨보다 낮으면? ➔ 진열 패스!
            if (currentLevel < item.unlockLevel) continue;

            GameObject go = Instantiate(shopItemPrefab, contentTransform);
            ShopItemUI uiItem = go.GetComponent<ShopItemUI>();

            if (uiItem != null)
            {
                uiItem.SetupItem(item);
            }
        }
    }
}