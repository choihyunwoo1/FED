using UnityEngine;

public class PopSave : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false); // 시작할 땐 숨겨둠
    }

    // 💡 [Save] (초록색) 버튼을 누르면 실행될 함수
    public void OnClickSaveMode()
    {
        if (PopSlotList.Instance != null)
        {
            // 1. 만능 슬롯창에게 '세이브 모드(true)'로 열라고 지시합니다!
            PopSlotList.Instance.OpenSlotList(true);

            // 2. 이 미니 팝업은 할 일을 다 했으니 화면에서 깔끔하게 숨겨줍니다.
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("❌ PopSlotList 매니저를 찾을 수 없습니다! 씬에 있는지 확인해주세요.");
        }
    }

    // 💡 [Load] (빨간색) 버튼을 누르면 실행될 함수
    public void OnClickLoadMode()
    {
        if (PopSlotList.Instance != null)
        {
            // 1. 만능 슬롯창에게 '로드 모드(false)'로 열라고 지시합니다!
            PopSlotList.Instance.OpenSlotList(false);

            // 2. 이 미니 팝업 숨기기
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("❌ PopSlotList 매니저를 찾을 수 없습니다! 씬에 있는지 확인해주세요.");
        }
    }

    // ❌ 우측 상단 [X] 버튼을 누르면 실행될 함수
    public void OnClickClose()
    {
        // 아무것도 안 하고 그냥 창만 닫습니다.
        gameObject.SetActive(false);
    }
}