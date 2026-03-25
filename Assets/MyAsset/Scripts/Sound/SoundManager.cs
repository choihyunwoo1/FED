using UnityEngine;
using System.Collections.Generic;

// 💡 유니티 인스펙터에서 소리 파일과 이름을 짝지어줄 수 있는 보관함
[System.Serializable]
public class Sound
{
    public string soundName; // 소리 이름 (예: "Click", "Buy", "Error")
    public AudioClip clip;   // 실제 소리 파일 (.mp3, .wav)
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("사운드 플레이어 (스피커)")]
    public AudioSource bgmPlayer; // BGM 전용 스피커 (반복 재생용)
    public AudioSource sfxPlayer; // SFX 전용 스피커 (효과음 겹쳐 틀기용)

    [Header("사운드 리스트 (여기에 파일 등록)")]
    public List<Sound> bgmList = new List<Sound>();
    public List<Sound> sfxList = new List<Sound>();

    // 빠른 검색을 위한 사전(Dictionary)
    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // 💡 씬이 넘어가도 노래가 끊기지 않게 불사신으로 만듭니다!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 게임 시작 전 리스트에 있는 소리들을 사전에 싹 정리해둡니다.
        foreach (Sound s in bgmList) bgmDict.Add(s.soundName, s.clip);
        foreach (Sound s in sfxList) sfxDict.Add(s.soundName, s.clip);
    }

    // ==========================================
    // 🔊 1. BGM (배경음악) 재생
    // ==========================================
    public void PlayBGM(string name)
    {
        if (bgmDict.ContainsKey(name))
        {
            bgmPlayer.clip = bgmDict[name];
            bgmPlayer.loop = true; // 배경음악은 무한 반복!
            bgmPlayer.Play();
        }
        else
        {
            Debug.LogWarning($"❌ BGM [{name}]을 찾을 수 없습니다!");
        }
    }

    // 💡 BGM 정지
    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    // ==========================================
    // 💥 2. SFX (효과음) 재생
    // ==========================================
    public void PlaySFX(string name)
    {
        if (sfxDict.ContainsKey(name))
        {
            // PlayOneShot을 쓰면 효과음이 여러 번 겹쳐도 씹히지 않고 동시에 납니다! (타격감 핵심)
            sfxPlayer.PlayOneShot(sfxDict[name]);
        }
        else
        {
            Debug.LogWarning($"❌ SFX [{name}]을 찾을 수 없습니다!");
        }
    }

    // ==========================================
    // 🎛️ 3. 볼륨 조절 (나중에 세팅 창에서 쓸 함수들)
    // ==========================================
    public void SetBgmVolume(float volume)
    {
        bgmPlayer.volume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxPlayer.volume = volume;
    }
}