using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("현재 설정값")]
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    // 💡 BGM과 SFX 뮤트(음소거)를 따로 분리했습니다!
    public bool isBgmMuted = false;
    public bool isSfxMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("BGM_Volume", bgmVolume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);

        // 💡 뮤트 여부도 각각 따로 저장! (bool은 저장이 안 돼서 1과 0으로 꼼수 저장)
        PlayerPrefs.SetInt("BGM_Mute", isBgmMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFX_Mute", isSfxMuted ? 1 : 0);

        PlayerPrefs.Save();
        ApplySettings();
        Debug.Log("⚙️ [세팅] BGM/SFX 분리 환경설정이 저장되었습니다.");
    }

    public void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 1f);
        isBgmMuted = PlayerPrefs.GetInt("BGM_Mute", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SFX_Mute", 0) == 1;

        ApplySettings();
    }

    public void ApplySettings()
    {
        // 💡 나중에 SoundManager를 만드시면, 여기서 실시간으로 볼륨을 쏴주면 됩니다!
        // 예: if (SoundManager.Instance != null) SoundManager.Instance.UpdateVolumes();
    }

    // ==========================================
    // 📢 외부에서 소리 크기를 물어볼 때 대답해주는 함수들
    // ==========================================

    // "BGM 소리 얼마나 크게 틀어야 돼?"
    public float GetRealBgmVolume()
    {
        // 뮤트가 켜져 있으면 0, 아니면 슬라이더 볼륨값을 반환!
        return isBgmMuted ? 0f : bgmVolume;
    }

    // "효과음 소리 얼마나 크게 틀어야 돼?"
    public float GetRealSfxVolume()
    {
        return isSfxMuted ? 0f : sfxVolume;
    }
}