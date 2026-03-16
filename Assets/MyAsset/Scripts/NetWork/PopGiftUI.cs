using UnityEngine;
using System.Collections.Generic;

public class PopGiftUI : MonoBehaviour
{
    public GameObject giftItemPrefab;  // 방금 만든 GiftItem 프리팹
    public Transform contentTransform; // Scroll View의 Content
    public GameObject emptyStateText;  // "가방에 선물이 없습니다" 텍스트

    private void OnEnable()
    {
        RefreshGiftList();
    }

    public void RefreshGiftList()
    {
        // 1. 기존 목록 싹 청소하기
        foreach (Transform child in contentTransform) Destroy(child.gameObject);

        // 2. 가방이 비어있는지 체크
        if (PlayerManager.Instance == null || PlayerManager.Instance.inventory.Count == 0)
        {
            if (emptyStateText != null) emptyStateText.SetActive(true);
            return;
        }

        if (emptyStateText != null) emptyStateText.SetActive(false);

        // 3. 내 가방(inventory)을 뒤져서 프리팹 생성!
        foreach (KeyValuePair<string, int> item in PlayerManager.Instance.inventory)
        {
            string itemName = item.Key;
            int itemAmount = item.Value;

            // 도감에서 이 아이템의 상세 스펙(호감도, 아이콘 등)을 가져옵니다.
            ItemData itemInfo = PlayerManager.Instance.GetItemInfo(itemName);

            GameObject go = Instantiate(giftItemPrefab, contentTransform);
            GiftItemUI uiItem = go.GetComponent<GiftItemUI>();

            if (uiItem != null)
            {
                // 프리팹에게 이름, 개수, 상세 스펙을 전부 쏴줍니다!
                uiItem.SetupItem(itemName, itemAmount, itemInfo);
            }
        }
    }
}