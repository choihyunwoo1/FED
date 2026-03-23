using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class PopBank : MonoBehaviour
{
    [Header("NPC 대화창 UI")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcDialogueText;

    [Header("서랍장 (예금 패널) UI")]
    public GameObject depositPanel;
    public TextMeshProUGUI currentMoneyText;
    public TextMeshProUGUI interestRateText;
    public TextMeshProUGUI bankDepositText;
    public TMP_InputField depositInputField;

    // 💡 [새로 추가된 대출 전용 서랍장 UI!]
    [Header("서랍장 (대출 패널) UI")]
    public GameObject debtPanel;                  // 대출용 서랍장
    public TextMeshProUGUI currentMoneyDebtText;  // 내 지갑 잔고
    public TextMeshProUGUI debtRateText;          // 대출 금리
    public TextMeshProUGUI remainLoanLimitText;   // 남은 대출 한도
    public TMP_InputField debtInputField;         // 대출 입력창

    [Header("NPC 잡담 대사 모음")]
    public List<string> randomTalks = new List<string>()
    {
        "비가 오나 눈이 오나, 이자는 꼬박꼬박 나갑니다. 명심하세요.",
        "주식 시장이 안 좋다고요? 저희 은행 예금은 아주 안전합니다. (아마도요)",
        "대출금 상환일은 매주 7일차입니다. 연체하시면... 아시죠?",
        "저희 은행 VIP가 되시면 특별한 혜택이... 아, 아직 평민이시군요.",
        "돈이 돈을 낳는 법이죠. 예금도 아주 훌륭한 투자입니다."
    };

    private string npcName = "친절한 은행원";
    private Coroutine resetCoroutine;

    private void OnEnable()
    {
        if (npcNameText != null) npcNameText.text = npcName;

        // 💡 창 열 때 서랍장 두 개 다 확실히 닫아두기
        if (depositPanel != null) depositPanel.SetActive(false);
        if (debtPanel != null) debtPanel.SetActive(false);

        ShowDefaultDialogue();
    }

    public void ShowDefaultDialogue()
    {
        if (npcDialogueText != null)
        {
            long deposit = PlayerManager.Instance.bankDeposit;
            long debt = PlayerManager.Instance.bankDebt;
            npcDialogueText.text = $"어서 오십시오.\n현재 고객님의 상환하실 대출금은 <color=red>{debt:N0}원</color>,\n예금은 <color=green>{deposit:N0}원</color> 있습니다.";
        }
    }

    private IEnumerator ResetDialogueAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDefaultDialogue();
    }

    private void UpdateDialogueWithTimer(string newText)
    {
        npcDialogueText.text = newText;
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
        resetCoroutine = StartCoroutine(ResetDialogueAfterSeconds(3f));
    }

    public void OnClickTalk()
    {
        if (npcDialogueText != null && randomTalks.Count > 0)
        {
            int randomIndex = Random.Range(0, randomTalks.Count);
            UpdateDialogueWithTimer(randomTalks[randomIndex]);
        }
    }

    // ==========================================
    // 📂 [서랍장 - 예금] 
    // ==========================================
    public void OnClickOpenBankPanel()
    {
        if (debtPanel != null) debtPanel.SetActive(false); // 대출창 열려있으면 끄기
        if (depositPanel != null)
        {
            depositPanel.SetActive(true);
            RefreshDepositPanel();
            UpdateDialogueWithTimer("예금 업무를 도와드리겠습니다. 금액을 입력해 주세요.");
        }
    }

    private void RefreshDepositPanel()
    {
        if (currentMoneyText != null) currentMoneyText.text = $"{PlayerManager.Instance.money:N0} 원";
        if (interestRateText != null) interestRateText.text = "주간 예금 1.0%";
        if (bankDepositText != null) bankDepositText.text = $"{PlayerManager.Instance.bankDeposit:N0} 원";
        if (depositInputField != null) depositInputField.text = "";
    }

    public void OnClickCustomDeposit()
    {
        if (depositInputField == null) return;

        if (long.TryParse(depositInputField.text, out long inputAmount))
        {
            if (inputAmount <= 0)
            {
                UpdateDialogueWithTimer("고객님, 0원 이하는 입금하실 수 없습니다.");
                return;
            }

            if (PlayerManager.Instance.SubtractMoney(inputAmount, false))
            {
                PlayerManager.Instance.bankDeposit += inputAmount;
                UpdateDialogueWithTimer($"[입금 완료]\n{inputAmount:N0}원이 안전하게 예금되었습니다.\n현재 예금: <color=green>{PlayerManager.Instance.bankDeposit:N0}원</color>");
                RefreshDepositPanel();
            }
            else
            {
                UpdateDialogueWithTimer("고객님, 지갑에 현금이 부족하신 것 같습니다만...?");
            }
        }
        else UpdateDialogueWithTimer("올바른 금액(숫자)을 입력해 주십시오.");
    }

    // ==========================================
    // 💸 [직접 입력 출금] 출금 버튼을 눌렀을 때
    // ==========================================
    public void OnClickCustomWithdraw()
    {
        if (depositInputField == null) return;

        // 1. 유저가 입력한 글씨를 숫자로 변환
        if (long.TryParse(depositInputField.text, out long inputAmount))
        {
            // 2. 0원 이하 장난 방지
            if (inputAmount <= 0)
            {
                UpdateDialogueWithTimer("고객님, 0원 이하는 출금하실 수 없습니다.");
                return;
            }

            // 3. 맡겨둔 예금이 충분한지 확인!
            if (PlayerManager.Instance.bankDeposit >= inputAmount)
            {
                // 은행에서 돈을 빼고, 내 지갑(money)에 더해줍니다.
                PlayerManager.Instance.bankDeposit -= inputAmount;
                PlayerManager.Instance.AddMoney(inputAmount, false); // 수입이 아니므로 가계부 기록 X

                UpdateDialogueWithTimer($"[출금 완료]\n{inputAmount:N0}원을 출금해 드렸습니다.\n남은 예금: <color=green>{PlayerManager.Instance.bankDeposit:N0}원</color>");

                // 💡 잔고가 바뀌었으니 서랍장 글씨도 최신화!
                RefreshDepositPanel();
            }
            else
            {
                UpdateDialogueWithTimer("고객님, 맡기신 예금보다 큰 금액은 찾으실 수 없습니다.");
            }
        }
        else
        {
            UpdateDialogueWithTimer("올바른 금액(숫자)을 입력해 주십시오.");
        }
    }

    // ==========================================
    // 📂 [서랍장 - 대출] 
    // ==========================================
    public void OnClickOpenDebtPanel()
    {
        if (depositPanel != null) depositPanel.SetActive(false); // 예금창 열려있으면 끄기
        if (debtPanel != null)
        {
            debtPanel.SetActive(true);
            RefreshDebtPanel();
            UpdateDialogueWithTimer("대출 업무 창구입니다. 원하시는 금액을 입력해 주세요.");
        }
    }

    private void RefreshDebtPanel()
    {
        if (currentMoneyDebtText != null) currentMoneyDebtText.text = $"{PlayerManager.Instance.money:N0} 원";
        if (debtRateText != null) debtRateText.text = "주간 대출 5.0%";

        // 💡 최대 대출 한도에서 내가 이미 빌린 돈을 빼서 '남은 한도'를 보여줍니다!
        long remainLimit = PlayerManager.Instance.maxLoanLimit - PlayerManager.Instance.bankDebt;
        if (remainLoanLimitText != null) remainLoanLimitText.text = $"{remainLimit:N0} 원";

        if (debtInputField != null) debtInputField.text = "";
    }

    // 💳 [직접 입력 대출] 대출받기 버튼을 눌렀을 때
    public void OnClickCustomTakeLoan()
    {
        if (debtInputField == null) return;

        if (long.TryParse(debtInputField.text, out long inputAmount))
        {
            if (inputAmount <= 0)
            {
                UpdateDialogueWithTimer("고객님, 0원 이하는 대출하실 수 없습니다.");
                return;
            }

            if (PlayerManager.Instance.bankDebt + inputAmount > PlayerManager.Instance.maxLoanLimit)
            {
                UpdateDialogueWithTimer("죄송하지만 고객님의 신용 한도를 초과하여 더 이상 대출이 불가합니다.");
                return;
            }

            PlayerManager.Instance.bankDebt += inputAmount;
            PlayerManager.Instance.AddMoney(inputAmount, false);
            UpdateDialogueWithTimer($"[대출 승인]\n{inputAmount:N0}원 대출이 완료되었습니다.\n총 대출금: <color=red>{PlayerManager.Instance.bankDebt:N0}원</color>");
            RefreshDebtPanel();
        }
        else UpdateDialogueWithTimer("올바른 금액(숫자)을 입력해 주십시오.");
    }

    // 🤝 [직접 입력 상환] 상환하기 버튼을 눌렀을 때
    public void OnClickCustomRepayLoan()
    {
        if (debtInputField == null) return;

        if (long.TryParse(debtInputField.text, out long inputAmount))
        {
            if (inputAmount <= 0) return;

            if (PlayerManager.Instance.bankDebt <= 0)
            {
                UpdateDialogueWithTimer("고객님은 현재 갚으실 대출금이 없습니다. 훌륭하시군요!");
                return;
            }

            // 빚보다 많이 적었으면 딱 남은 빚까지만 상환하게 조절
            long actualRepay = System.Math.Min(inputAmount, PlayerManager.Instance.bankDebt);

            if (PlayerManager.Instance.SubtractMoney(actualRepay, false))
            {
                PlayerManager.Instance.bankDebt -= actualRepay;
                UpdateDialogueWithTimer($"[상환 완료]\n대출금 {actualRepay:N0}원을 상환하셨습니다.\n남은 대출금: <color=red>{PlayerManager.Instance.bankDebt:N0}원</color>");
                RefreshDebtPanel();
            }
            else
            {
                UpdateDialogueWithTimer("고객님, 상환하실 현금이 부족합니다.");
            }
        }
        else UpdateDialogueWithTimer("올바른 금액(숫자)을 입력해 주십시오.");
    }

    public void OnClickClose()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
    }

    public void OnClickCloseDrawer()
    {
        // 1. 열려있는 서랍장들을 조용히 닫습니다. (비서 몰래!)
        if (depositPanel != null) depositPanel.SetActive(false);
        if (debtPanel != null) debtPanel.SetActive(false);

        // 2. 서랍장을 닫았으니, 다시 처음의 기본 인사말로 되돌려줍니다.
        UpdateDialogueWithTimer("다른 업무를 도와드릴까요?");
    }
}