using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 싱글톤 패턴 (어디서든 UIManager.Instance로 접근 가능)
    public static UIManager Instance;

    // 🗂️ 캐시: 한 번 생성된 UI를 저장해두는 보관함
    private Dictionary<string, GameObject> panelCache = new Dictionary<string, GameObject>();

    // 📚 스택: 유저가 거쳐온 UI의 '방문 기록' (뒤로 가기를 위해)
    private Stack<GameObject> panelStack = new Stack<GameObject>();

    [SerializeField] private GameObject popUpUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 🚀 새로운 UI 패널 열기
    public void OpenPanel(string panelName)
    {
        // 🚨 [스팸 클릭 방지 방어막]
        // 열려고 하는 창이 이미 캐시에 존재하고, 현재 켜져있는(스택 맨 위) 창과 똑같다면 무시합니다!
        if (panelCache.ContainsKey(panelName) && panelStack.Count > 0)
        {
            if (panelStack.Peek() == panelCache[panelName])
            {
                Debug.Log($"🛑 [{panelName}] 이미 켜져 있는 창이라 중복으로 열지 않습니다!");
                return; // 함수를 여기서 바로 종료시켜버림!
            }
        }

        // 1. 현재 켜져 있는 창이 있다면 숨기기 (스택의 맨 위 확인)
        if (panelStack.Count > 0)
        {
            GameObject currentPanel = panelStack.Peek(); // Peek: 빼지 않고 맨 위 항목만 확인
            currentPanel.SetActive(false);
        }

        GameObject newPanel = null;

        // 2. 캐시(보관함)에 패널이 있는지 확인
        if (panelCache.ContainsKey(panelName))
        {
            newPanel = panelCache[panelName];
        }
        else
        {
            // 💡 캐시에 없다면 새로 불러와서 생성해야 합니다.
            GameObject prefab = Resources.Load<GameObject>(panelName);
            if (prefab != null)
            {
                newPanel = Instantiate(prefab, popUpUI.transform, false); // 화면에 생성
                panelCache.Add(panelName, newPanel);       // 다음을 위해 캐시에 저장
            }
            else
            {
                Debug.LogError(panelName + " 프리팹을 찾을 수 없습니다!");
            }
        }

        // 3. 새 패널을 스택에 추가(Push)하고 화면에 켜기
        if (newPanel != null)
        {
            panelStack.Push(newPanel);
            newPanel.SetActive(true);
        }
    }

    // 🔙 뒤로 가기 (현재 패널 닫기)
    public void CloseCurrentPanel()
    {
        // 스택에 닫을 창이 남아있는지 확인
        if (panelStack.Count > 0)
        {
            // 1. 스택 맨 위의 창을 빼내서(Pop) 끄기
            GameObject topPanel = panelStack.Pop();
            topPanel.SetActive(false);

            // 2. 빼내고 났더니 그 아래에 이전 창이 남아있다면 다시 켜주기
            if (panelStack.Count > 0)
            {
                GameObject previousPanel = panelStack.Peek();
                previousPanel.SetActive(true);
            }
        }
    }

    // 💥 모든 패널 한 번에 닫기 (시간 이동, 강제 이벤트 등에서 사용!)
    public void CloseAllPanels()
    {
        // 스택에 창이 남아있는 동안 계속해서 빼내고 끕니다.
        while (panelStack.Count > 0)
        {
            GameObject panel = panelStack.Pop();
            panel.SetActive(false);
        }
    }
}