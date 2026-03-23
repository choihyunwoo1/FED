using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PopSlotList : MonoBehaviour
{
    public static PopSlotList Instance;

    [Header("UI 연결")]
    public TextMeshProUGUI titleText;
    public List<SaveSlotUI> slotUIs;

    private bool isSaveMode = true;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenSlotList(bool saveMode)
    {
        isSaveMode = saveMode;

        // 🚨 수정 1: 혼자 켜지는 SetActive(true) 삭제! (이제 비서가 켜주니까요)

        if (isSaveMode)
            titleText.text = "어디에 저장하시겠습니까? (Save)";
        else
            titleText.text = "어떤 기록을 부르시겠습니까? (Load)";

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null) slotUIs[i].UpdateVisuals(i);
        }
    }

    public void OnClickSlot(int slotNumber)
    {
        if (isSaveMode)
        {
            PopConfirm.Instance.SetupMessage("저장 확인", $"{slotNumber}번 슬롯에 저장할까요?", () =>
            {
                SaveManager.Instance.SaveGame(slotNumber);
                RefreshSlots();

                // 🚨 수정 2: 저장 성공 시 CurrentPanel 대신 AllPanels로 싹 닫기!
                if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();
                else gameObject.SetActive(false);
            });
        }
        else
        {
            if (SaveManager.Instance.HasSaveData(slotNumber))
            {
                PopConfirm.Instance.SetupMessage("불러오기 확인", $"{slotNumber}번 슬롯을 불러올까요?", () =>
                {
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene")
                    {
                        PlayerPrefs.SetInt("IsLoadGame", 1);
                        PlayerPrefs.SetInt("LoadSlotNumber", slotNumber);

                        if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene("PlayScene");
                    }
                    else
                    {
                        SaveManager.Instance.LoadGame(slotNumber);

                        // 🚨 수정 3: 로드 성공 시 CurrentPanel 대신 AllPanels로 싹 닫기!
                        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();
                        else gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                PopConfirm.Instance.SetupMessage("알림", "빈 슬롯이라 불러올 수 없습니다!", null);
            }
        }
    }

    public void OnClickClose()
    {
        // 취소(X버튼)할 때는 원래대로 이전 창(SaveLoad 창)으로 돌아가야 하므로 이건 그대로 둡니다!
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false);
    }

    // 🚨 수정 4: 맨 아래 있던 OpenForSave(), OpenForLoad() 함수는 이제 PopSave.cs가 알아서 열어주므로 깔끔하게 지웠습니다!
}