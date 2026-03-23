using UnityEngine;
using UnityEngine.UI;

public class PopSettings : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 💡 뮤트 토글도 2개로 분리!
    public Toggle bgmMuteToggle;
    public Toggle sfxMuteToggle;

    private void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            if (bgmSlider != null) bgmSlider.value = SettingsManager.Instance.bgmVolume;
            if (sfxSlider != null) sfxSlider.value = SettingsManager.Instance.sfxVolume;
            if (bgmMuteToggle != null) bgmMuteToggle.isOn = SettingsManager.Instance.isBgmMuted;
            if (sfxMuteToggle != null) sfxMuteToggle.isOn = SettingsManager.Instance.isSfxMuted;
        }
    }

    public void OnBgmSliderChanged()
    {
        SettingsManager.Instance.bgmVolume = bgmSlider.value;
        SettingsManager.Instance.ApplySettings();
    }

    public void OnSfxSliderChanged()
    {
        SettingsManager.Instance.sfxVolume = sfxSlider.value;
        SettingsManager.Instance.ApplySettings();
    }

    // 💡 BGM 뮤트 토글용 함수
    public void OnBgmMuteToggleChanged()
    {
        SettingsManager.Instance.isBgmMuted = bgmMuteToggle.isOn;
        SettingsManager.Instance.ApplySettings();
    }

    // 💡 SFX 뮤트 토글용 함수
    public void OnSfxMuteToggleChanged()
    {
        SettingsManager.Instance.isSfxMuted = sfxMuteToggle.isOn;
        SettingsManager.Instance.ApplySettings();
    }

    public void OnClickSaveAndClose()
    {
        SettingsManager.Instance.SaveSettings();

        if (UIManager.Instance != null) UIManager.Instance.CloseCurrentPanel();
        else gameObject.SetActive(false);
    }

    // ==========================================
    // 💡 [플레이 씬 전용] 메인 메뉴(타이틀)로 돌아가기 버튼
    // ==========================================
    public void OnClickReturnToTitle()
    {
        // 그냥 튕기면 유저가 화낼 수 있으니, 친절하게 PopConfirm으로 물어봅니다!
        if (PopConfirm.Instance != null)
        {
            PopConfirm.Instance.SetupMessage(
                "메인 메뉴로",
                "저장하지 않은 진행 상황은 모두 잃게 됩니다.\n정말 메인 메뉴로 돌아가시겠습니까?",
                ExecuteReturnToTitle // 👈 확인(YES) 누르면 이 함수 실행!
            );
        }
        else
        {
            // 만약 팝업창이 없으면 그냥 바로 이동
            ExecuteReturnToTitle();
        }
    }

    private void ExecuteReturnToTitle()
    {
        Debug.Log("🚀 타이틀 씬으로 돌아갑니다!");

        // 1. 혹시 켜져 있는 다른 UI 창들이 있다면 싹 다 닫아줍니다.
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        // 2. 우리의 만능 장막(SceneFader)을 불러서 타이틀 씬으로 멋지게 이동!
        // ⚠️ 주의: "TitleScene" 부분은 유저님의 실제 타이틀 씬 이름과 똑같아야 합니다!
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("TitleScene");
        }
        else
        {
            // 혹시 페이더가 없으면 기본 유니티 방식으로 강제 이동
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }
    }

    // ==========================================
    // 💡 [추가] 세이브 데이터 초기화 버튼
    // ==========================================
    public void OnClickResetSave()
    {
        if (PopConfirm.Instance != null)
        {
            PopConfirm.Instance.SetupMessage(
                "세이브 초기화",
                "모든 진행 상황이 삭제되며 타이틀 화면으로 돌아갑니다.\n이 작업은 되돌릴 수 없습니다. 삭제하시겠습니까?",
                ExecuteResetSave // 👈 확인 누르면 아래 함수 실행!
            );
        }
    }

    private void ExecuteResetSave()
    {
        Debug.Log("🗑️ 모든 세이브 데이터를 삭제합니다!");

        // 1. 반복문(for)을 써서 모든 슬롯을 모조리 찾아내서 폭파시킵니다!
        if (SaveManager.Instance != null)
        {
            for (int i = 0; i < 4; i++) // 슬롯이 4개니까 0부터 3까지! (만약 슬롯 늘리시면 이 숫자도 늘려주세요)
            {
                SaveManager.Instance.DeleteSaveData(i);
            }
        }

        // 2. 켜져 있는 다른 UI 창 닫기
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        // 3. 타이틀 씬으로 쫓아내기!
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("TitleScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }
    }

    // ==========================================
    // 💡 [추가] 중도 포기 기능
    // ==========================================
    public void OnClickGiveUp()
    {
        if (PopConfirm.Instance != null)
        {
            // 확인 팝업을 먼저 띄워서 실수로 누르는 걸 방지합니다!
            PopConfirm.Instance.SetupMessage(
                "중도 포기",
                "정말로 투자를 포기하시겠습니까?\n모든 자산이 청산되며 즉시 게임이 종료됩니다.",
                ExecuteGiveUp // 👈 확인 누르면 아래 함수 실행!
            );
        }
    }

    private void ExecuteGiveUp()
    {
        // 💡 플레이어가 스스로 목숨을 끊었(?)으므로 파산 엔딩 강제 실행!
        if (EndingManager.Instance != null)
        {
            EndingManager.Instance.TriggerEnding(EndingType.Bankruptcy);
        }
    }
}