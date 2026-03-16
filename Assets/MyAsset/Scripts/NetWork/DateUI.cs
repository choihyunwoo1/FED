using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DateUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI affinityText; // 호감도 텍스트 (예: "호감도: 20/100")

    [Header("서랍장 UI")]
    public GameObject giftDrawerPanel;

    private NPCData targetNPC;

    // 💡 창이 열릴 때 자동으로 현재 세팅된 NPC 정보를 불러와서 화면에 그림!
    // 💡 [버그 픽스 1] 창이 열릴 때마다 무조건 서랍장을 닫아버립니다!
    private void OnEnable()
    {
        if (giftDrawerPanel != null)
        {
            giftDrawerPanel.SetActive(false); // 서랍장 강제 닫기!
        }

        // ... (아래는 기존에 있던 타겟 NPC 불러오고 UI 업데이트하는 코드 유지) ...
        targetNPC = DateManager.Instance.GetCurrentNPC();
        UpdateUI();
        if (dialogueText != null) dialogueText.text = $"[{targetNPC.npcName}] 어서오세요. 무슨 일이시죠?";
    }

    private void UpdateUI()
    {
        if (targetNPC == null) return;

        nameText.text = targetNPC.npcName;
        portraitImage.sprite = targetNPC.portrait;
        affinityText.text = $"호감도: {targetNPC.currentAffinity} / {targetNPC.maxAffinity}";
    }

    // 💬 [버튼 연결] 대화하기 버튼 
    public void OnClickTalk()
    {
        // 등록된 일상 대화 중 하나를 랜덤으로 뽑아서 출력!
        if (targetNPC.normalDialogues.Count > 0)
        {
            int randomIndex = Random.Range(0, targetNPC.normalDialogues.Count);
            dialogueText.text = targetNPC.normalDialogues[randomIndex];
        }
    }

    // 🎁 선물하기 버튼을 누르면 실행!
    public void OnClickOpenGiftMenu()
    {
        // 서랍장이 꺼져있으면 켜고, 켜져있으면 끄는 마법의 토글 스위치!
        bool isDrawerOpen = !giftDrawerPanel.activeSelf;
        giftDrawerPanel.SetActive(isDrawerOpen);

        // 💡 서랍장을 열 때마다 가방 안의 아이템 목록을 새로고침 해줍니다!
        if (isDrawerOpen)
        {
            PopGiftUI giftUI = giftDrawerPanel.GetComponent<PopGiftUI>();
            if (giftUI != null) giftUI.RefreshGiftList();
        }
    }

    // 💡 [버그 픽스 2] 서랍장 우측 상단의 [X] 버튼을 누르면 실행될 함수!
    public void OnClickCloseGiftDrawer()
    {
        if (giftDrawerPanel != null)
        {
            giftDrawerPanel.SetActive(false); // UIManager를 부르지 않고 서랍장만 쏙 닫습니다.
        }
    }

    // 💡 선물 목록 창에서 아이템을 고르면 실행되는 함수
    public void GiveGift(string giftName)
    {
        if (PlayerManager.Instance.HasItem(giftName))
        {
            ItemData itemInfo = PlayerManager.Instance.GetItemInfo(giftName);
            int bonus = (itemInfo != null) ? itemInfo.affinityBonus : 0;

            PlayerManager.Instance.RemoveItem(giftName, 1);

            targetNPC.currentAffinity += bonus;
            targetNPC.currentAffinity = Mathf.Min(targetNPC.currentAffinity, targetNPC.maxAffinity);

            dialogueText.text = $"앗, 이 [{giftName}]...! 정말 감사합니다! (호감도 +{bonus} 상승)";
            UpdateUI();
            CheckAffinityReward();

            // 💡 [핵심] 선물을 줬으니 반절짜리 서랍장을 다시 닫아줍니다!
            giftDrawerPanel.SetActive(false);
        }
    }

    // 🔍 정보 보상 지급 함수
    private void CheckAffinityReward()
    {
        if (!targetNPC.hasGivenSecret && targetNPC.currentAffinity >= targetNPC.rewardThreshold)
        {
            targetNPC.hasGivenSecret = true; // 도장 쾅! (다신 안 줌)

            // 우리가 기존에 만들어둔 스토리 컷씬을 재활용해서 정보를 띄워버립니다!
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenPanel("PopEventCutScene");

                EventCutSceneUI popup = FindAnyObjectByType<EventCutSceneUI>();
                if (popup != null)
                {
                    popup.SetupStory(targetNPC.portrait, $"<color=yellow>[특급 정보 입수!]</color>\n\n{targetNPC.secretInfoText}");
                }
            }
        }
    }
}