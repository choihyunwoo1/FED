using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI infoText; // "빈 슬롯" 또는 "Day 15 / 100만 원"
    public Image buttonBg;           // 버튼 배경 이미지

    [Header("색상 설정")]
    public Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 1f); // 회색 (빈 슬롯)
    public Color savedColor = new Color(1f, 1f, 1f, 1f);       // 흰색 (저장된 슬롯)

    // 💡 팝업창(PopSlotList)이 이 함수를 부르면서 몇 번 슬롯인지(0, 1, 2) 알려줍니다!
    public void UpdateVisuals(int slotNumber)
    {
        if (SaveManager.Instance.HasSaveData(slotNumber))
        {
            // 데이터가 있으면 아까 만든 '미리보기' 함수로 겉표지만 가져옵니다!
            SaveData preview = SaveManager.Instance.GetSaveDataPreview(slotNumber);
            if (preview != null)
            {
                infoText.text = $"Slot {slotNumber}\nDay {preview.currentDay} / {preview.money:N0}원";
                buttonBg.color = savedColor;
            }
        }
        else
        {
            // 데이터가 없으면
            infoText.text = $"Slot {slotNumber}\n[ 빈 슬롯 ]";
            buttonBg.color = emptyColor;
        }
    }
}