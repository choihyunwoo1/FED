using UnityEngine;

public class TitleManager : MonoBehaviour
{
    // 💡 [새 게임] 버튼을 눌렀을 때
    public void OnClickNewGame()
    {
        // 1번 슬롯에 저장된 세이브 파일이 있다면? (경고 팝업 띄우기)
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData(1))
        {
            // (주의: PopConfirm 프리팹이 씬에 있어야 합니다!)
            PopConfirm.Instance.SetupMessage("새 게임", "기존 저장 데이터가 지워집니다.\n새로 시작하시겠습니까?", StartNewGame);
        }
        else
        {
            StartNewGame(); // 세이브가 없으면 그냥 바로 시작!
        }
    }

    // 실제 새 게임을 시작하는 함수 (장막 페이더 호출!)
    private void StartNewGame()
    {
        Debug.Log("🚀 새 게임을 시작합니다!");
        // (필요하다면 여기서 기존 세이브 파일을 지우는 코드를 넣어도 좋습니다)

        SceneFader.Instance.FadeToScene("PlayScene"); // PlayScene으로 이동!
    }

    // 💡 [이어하기] 버튼을 눌렀을 때
    public void OnClickLoadGame()
    {
        if (UIManager.Instance != null)
        {
            // 1. 비서를 시켜서 일단 창을 켭니다.
            UIManager.Instance.OpenPanel("PopSlotList");

            // 2. 💡 켜진 창한테 "너 로드 모드(false)로 글씨 싹 새로고침 해!" 라고 직접 명령합니다.
            if (PopSlotList.Instance != null)
            {
                PopSlotList.Instance.OpenSlotList(false);
            }
        }
    }

    // 💡 [게임 종료] 버튼을 눌렀을 때
    public void OnClickExit()
    {
        PopConfirm.Instance.SetupMessage("게임 종료", "게임을 종료하시겠습니까?", () => {
            Debug.Log("게임을 종료합니다.");
            Application.Quit(); // 실제 빌드된 게임에서만 작동함 (에디터에선 안 꺼짐)
        });
    }

    // 💡 [세팅] 버튼을 눌렀을 때
    public void OnClickSettings()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel("PopTitleSetting");
        }
    }
}