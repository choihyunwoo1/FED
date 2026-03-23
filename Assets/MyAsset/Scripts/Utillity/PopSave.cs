using UnityEngine;

public class PopSave : MonoBehaviour // 선택 팝업에 붙어있는 스크립트
{
    // 🔘 [Save] 버튼을 눌렀을 때
    public void OnClickSaveSelect()
    {
        // 🚨 1. 비서(UIManager)에게 슬롯 리스트 프리팹을Resources에서 띄우라고 지시! (이때 클론이 생성됨)
        if (UIManager.Instance != null)
        {
            // ⚠️ 주의: Resources 폴더 안에 "PopSlotList"라는 프리팹 이름과 똑같아야 합니다!
            UIManager.Instance.OpenPanel("PopSlotList");
        }

        // 🚨 2. 비서가 방금 띄운 그 클론(Instance)에게 "세이브 모드로 세팅해!"라고 명령!
        // (이제 Instance가 Null이 아니게 됩니다!)
        if (PopSlotList.Instance != null)
        {
            PopSlotList.Instance.OpenSlotList(true); // true = 저장 모드
        }
    }

    // 🔘 [Load] 버튼을 눌렀을 때
    public void OnClickLoadSelect()
    {
        // 1. 비서 시켜서 프리팹 띄우기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel("PopSlotList");
        }

        // 2. 클론에게 "로드 모드" 세팅!
        if (PopSlotList.Instance != null)
        {
            PopSlotList.Instance.OpenSlotList(false); // false = 로드 모드
        }
    }

    // [X] 닫기 버튼용 (기존 그대로)
    public void OnClickClose()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false);
    }
}