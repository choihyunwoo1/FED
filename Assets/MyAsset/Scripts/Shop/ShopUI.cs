using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("상점 NPC 정보")]
    public string shopkeeperName = "김상인";
    public Sprite shopkeeperPortrait;  // 💡 상점 이미지 소스
    public int currentShopLevel = 1;   // 💡 현재 상점 레벨 (나중에 매니저에서 불러와도 됨)
    public List<string> talkDialogues; // 일상 대화 목록

    [Header("💎 VIP 상점 레벨업 시스템")]
    public int maxShopLevel = 3;
    // 💡 레벨업에 필요한 돈 (0, 1렙은 이미 열려있으니 0원, 2렙 갈 때 50만 원, 3렙 갈 때 200만 원)
    public int[] levelUpCosts = { 0, 0, 500000, 2000000 };
    public TextMeshProUGUI levelUpBtnText; // 💡 [레벨업 버튼]에 띄워줄 글씨

    [Header("UI 연결")]
    public Image portraitImage;           // 💡 초상화가 들어갈 UI Image 컴포넌트
    public TextMeshProUGUI shopLevelText; // 💡 상점 레벨을 띄울 텍스트 컴포넌트
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("서랍장 2개 연결")]
    public GameObject shopDrawerPanel;      // 🛒 물건 사는 창 (PopShopUI)
    public GameObject inventoryDrawerPanel; // 🎒 내 가방 창 (PopInventoryUI)

    // 💡 창이 열릴 때 초기화
    private void OnEnable()
    {
        // 처음엔 서랍장 둘 다 닫아둡니다.
        if (shopDrawerPanel != null) shopDrawerPanel.SetActive(false);
        if (inventoryDrawerPanel != null) inventoryDrawerPanel.SetActive(false);

        if (nameText != null) nameText.text = shopkeeperName;
        if (portraitImage != null && shopkeeperPortrait != null) portraitImage.sprite = shopkeeperPortrait;
        UpdateLevelUI();
        if (dialogueText != null) dialogueText.text = "어서오세요. 비싼 물건만 취급합니다.";
    }

    // 💡 [새로 추가] 레벨 글씨와 버튼 글씨를 갱신해주는 함수
    private void UpdateLevelUI()
    {
        if (shopLevelText != null) shopLevelText.text = $"Lv.{currentShopLevel} 상점";

        if (levelUpBtnText != null)
        {
            if (currentShopLevel >= maxShopLevel)
                levelUpBtnText.text = "MAX LEVEL";
            else
                levelUpBtnText.text = $"VIP 승급\n({levelUpCosts[currentShopLevel + 1]:N0}원)";
        }
    }

    // 💎 [레벨업 버튼]을 누르면 실행될 함수!
    public void OnClickLevelUpShop()
    {
        if (currentShopLevel >= maxShopLevel)
        {
            dialogueText.text = "이미 최고 등급의 VIP 고객이십니다.";
            return;
        }

        int nextLevelCost = levelUpCosts[currentShopLevel + 1];

        // 지갑에서 레벨업 비용 빼기 시도!
        if (PlayerManager.Instance.SubtractMoney(nextLevelCost))
        {
            currentShopLevel++; // 빰빠밤~ 레벨업!
            dialogueText.text = $"감사합니다! 이제부터 VIP {currentShopLevel}단계의 은밀한 물건을 보여드리죠.";

            UpdateLevelUI(); // 간판 바꿔달기

            // 만약 상점 서랍장이 열려있다면, 진열대에 새 물건을 쫙 깔아줍니다!
            if (shopDrawerPanel != null && shopDrawerPanel.activeSelf)
            {
                PopShopUI shopUI = shopDrawerPanel.GetComponent<PopShopUI>();
                if (shopUI != null) shopUI.RefreshShop();
            }
        }
        else
        {
            dialogueText.text = $"(VIP 승급에는 {nextLevelCost:N0}원이 필요합니다. 돈이 부족하군...)";
        }
    }

    // 💬 [대화하기] 버튼
    public void OnClickTalk()
    {
        if (talkDialogues.Count > 0)
        {
            int rand = Random.Range(0, talkDialogues.Count);
            dialogueText.text = talkDialogues[rand];
        }
    }

    // 🛒 [구매(Shop)] 버튼
    public void OnClickOpenShop()
    {
        // 인벤토리가 켜져있다면 강제로 끕니다 (두 개가 겹치지 않게!)
        if (inventoryDrawerPanel != null) inventoryDrawerPanel.SetActive(false);

        bool isOpen = !shopDrawerPanel.activeSelf;
        shopDrawerPanel.SetActive(isOpen);

        if (isOpen)
        {
            dialogueText.text = "어떤 물건을 찾으십니까?";

            // 저번에 만든 상점 목록 새로고침 함수 실행!
            PopShopUI shopUI = shopDrawerPanel.GetComponent<PopShopUI>();
            if (shopUI != null) shopUI.RefreshShop();
        }
    }

    // 🎒 [가방(Inventory)] 버튼
    public void OnClickOpenInventory()
    {
        // 상점이 켜져있다면 강제로 끕니다
        if (shopDrawerPanel != null) shopDrawerPanel.SetActive(false);

        bool isOpen = !inventoryDrawerPanel.activeSelf;
        inventoryDrawerPanel.SetActive(isOpen);

        if (isOpen)
        {
            dialogueText.text = "손님의 가방을 확인합니다.";

            // 나중에 만들 인벤토리 새로고침 함수 실행 (일단 주석 처리)
            // PopInventoryUI invUI = inventoryDrawerPanel.GetComponent<PopInventoryUI>();
            // if (invUI != null) invUI.RefreshInventory();
        }
    }

    // ❌ [서랍장 닫기] (서랍장 우측 상단의 X버튼용)
    public void OnClickCloseDrawers()
    {
        if (shopDrawerPanel != null) shopDrawerPanel.SetActive(false);
        if (inventoryDrawerPanel != null) inventoryDrawerPanel.SetActive(false);
        dialogueText.text = "천천히 둘러보시지요.";
    }
}