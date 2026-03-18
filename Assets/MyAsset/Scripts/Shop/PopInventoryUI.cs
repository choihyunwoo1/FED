using UnityEngine;
using System.Collections.Generic;

public class PopInventoryUI : MonoBehaviour
{
    public GameObject inventoryItemPrefab; // 방금 만든 되팔기 기능이 있는 프리팹
    public Transform contentTransform;
    public GameObject emptyStateText;

    // 상점 UI에서 이 서랍장을 열 때마다 실행됨
    private void OnEnable()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        foreach (Transform child in contentTransform) Destroy(child.gameObject);

        if (PlayerManager.Instance == null || PlayerManager.Instance.inventory.Count == 0)
        {
            if (emptyStateText != null) emptyStateText.SetActive(true);
            return;
        }

        if (emptyStateText != null) emptyStateText.SetActive(false);

        foreach (KeyValuePair<string, int> item in PlayerManager.Instance.inventory)
        {
            string itemName = item.Key;
            int itemAmount = item.Value;
            ItemData itemInfo = PlayerManager.Instance.GetItemInfo(itemName);

            GameObject go = Instantiate(inventoryItemPrefab, contentTransform);
            InventoryItemUI uiItem = go.GetComponent<InventoryItemUI>();

            if (uiItem != null)
            {
                uiItem.SetupItem(itemName, itemAmount, itemInfo);
            }
        }
    }
}