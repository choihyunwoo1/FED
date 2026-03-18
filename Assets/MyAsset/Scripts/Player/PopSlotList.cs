using UnityEngine;
using TMPro;

public class PopSlotList : MonoBehaviour
{
    public static PopSlotList Instance;

    public TextMeshProUGUI titleText; // 상단 제목 ("Save" 또는 "Load")

    // 💡 현재 이 창이 무슨 모드인지 기억하는 변수! (true면 세이브, false면 로드)
    private bool isSaveMode = true;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    // 💡 [핵심] 아까 스샷의 Load / Save 버튼을 누를 때 이 함수를 부릅니다!
    public void OpenSlotList(bool saveMode)
    {
        isSaveMode = saveMode;
        gameObject.SetActive(true);

        if (isSaveMode)
            titleText.text = "어디에 저장하시겠습니까? (Save)";
        else
            titleText.text = "어떤 기록을 부르시겠습니까? (Load)";

        // TODO: 여기서 슬롯 1, 2, 3의 텍스트를 갱신해줍니다. (예: "빈 슬롯" or "Day 15")
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        // (나중에 슬롯 UI 스크립트를 만들어서 세팅할 부분)
        // 예: 1번 슬롯에 파일이 있으면 "데이터 있음", 없으면 "Empty" 글씨 쓰기
    }

    // 💡 슬롯 1번 버튼을 눌렀을 때 실행될 함수!
    public void OnClickSlot(int slotNumber)
    {
        if (isSaveMode)
        {
            // 💡 세이브 모드일 땐? ➔ 덮어쓸까요? 확인 팝업 띄우고 저장!
            PopConfirm.Instance.ShowPopup($"{slotNumber}번 슬롯에 저장할까요?", () =>
            {
                SaveManager.Instance.SaveGame(slotNumber);
                RefreshSlots(); // 저장했으니 슬롯 글씨 새로고침!
            });
        }
        else
        {
            // 💡 로드 모드일 땐? ➔ 데이터가 있는지 확인하고 불러오기!
            if (SaveManager.Instance.HasSaveData(slotNumber))
            {
                PopConfirm.Instance.ShowPopup($"{slotNumber}번 슬롯을 불러올까요?", () =>
                {
                    SaveManager.Instance.LoadGame(slotNumber);
                    gameObject.SetActive(false); // 다 불렀으니 창 닫기
                });
            }
            else
            {
                Debug.LogWarning("빈 슬롯이라 불러올 수 없습니다!");
            }
        }
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}