using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;       // 이름 (예: "캔커피", "명품 시계")
    public int affinityBonus;     // 이 아이템이 올려주는 호감도 수치! (예: 5, 50)
    public int price;             // 나중에 상점에서 팔 가격 (예: 1000, 500000)

    public Sprite itemIcon; // 나중에 UI에 그릴 아이콘 이미지
}