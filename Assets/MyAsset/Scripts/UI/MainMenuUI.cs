using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // 🔘 [네트워크] 버튼을 눌렀을 때
    public void OnClickNetworkButton()
    {
        // 1. DayManager한테 "지금 권한 있어?" 물어보기
        if (DayManager.Instance.CheckPermission("PopNetwork") == false)
        {
            Debug.Log("지금은 외출할 수 없습니다!");
            return; // 권한 없으면 여기서 함수 끝! (밑으로 안 내려감)
        }

        // 2. 권한이 있다면 UIManager(비서)에게 창을 열어달라고 명령!
        UIManager.Instance.OpenPanel("PopNetwork");
    }

    // 🔘 [마켓] 버튼을 눌렀을 때
    public void OnClickMarketButton()
    {
        // 1. DayManager한테 "지금 권한 있어?" 물어보기
        if (DayManager.Instance.CheckPermission("PopStockList") == false)
        {
            Debug.Log("지금은 장이 열리지 않아 마켓에 들어갈 수 없습니다!");
            // TODO: 나중에 여기에 "장이 닫혔습니다" 안내 팝업을 띄우면 완벽합니다!
            return; // 권한 없으면 여기서 함수 끝! (밑으로 안 내려감)
        }

        // 2. 권한이 있다면 UIManager(비서)에게 창을 열어달라고 명령!
        UIManager.Instance.OpenPanel("PopStockList");
    }

    // 🔘 [포트폴리오] 버튼을 눌렀을 때
    public void OnClickPortfolioButton()
    {
        if (DayManager.Instance.CheckPermission("PopPortfolio") == false)
        {
            Debug.Log("현재 포트폴리오를 열 수 없습니다.");
            return;
        }
        UIManager.Instance.OpenPanel("PopPortfolio");
    }

    // 🔘 [브리핑(뉴스)] 버튼을 눌렀을 때
    public void OnClickBriefingButton()
    {
        if (DayManager.Instance.CheckPermission("PopBriefing") == false)
        {
            Debug.Log("지금은 뉴스를 확인할 수 없습니다.");
            return;
        }
        UIManager.Instance.OpenPanel("PopBriefing");
    }

    // 🔘 [거버넌스] 버튼을 눌렀을 때
    public void OnClickGovernanceButton()
    {
        // 1. DayManager한테 "지금 권한 있어?" 물어보기
        if (DayManager.Instance.CheckPermission("PopGovernance") == false)
        {
            Debug.Log("지금은 로비할 수 없습니다!");
            return; // 권한 없으면 여기서 함수 끝! (밑으로 안 내려감)
        }

        // 2. 권한이 있다면 UIManager(비서)에게 창을 열어달라고 명령!
        UIManager.Instance.OpenPanel("PopGovernance");
    }

    // 🏪 상점 버튼을 누르면 실행!
    public void OnClickOpenShop()
    {
        if (UIManager.Instance != null)
        {
            // 비서에게 상점 창을 열라고 지시합니다.
            UIManager.Instance.OpenPanel("PopShop");
        }
    }

    // 세팅 버튼을 누르면 실행!
    public void OnClickOpenSetting()
    {
        if (UIManager.Instance != null)
        {
            // 비서에게 상점 창을 열라고 지시합니다.
            UIManager.Instance.OpenPanel("PopSetting");
        }
    }

    // 💡 [세이브/로드 버튼]을 눌렀을 때
    public void OnClickOpenSaveLoad()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel("PopSaveLoad"); // 프리팹 이름과 똑같이 맞춰주세요!
        }
    }

    // 은행 버튼을 누르면 실행!
    public void OnClickOpenBank()
    {
        if (UIManager.Instance != null)
        {
            // 비서에게 상점 창을 열라고 지시합니다.
            UIManager.Instance.OpenPanel("PopBank");
        }
    }
}