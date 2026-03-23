using UnityEngine;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    public TMP_Text username;
    public TMP_Text gold;
    public static PlayerInfo Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetUsername(string username)
    {
        this.username.text = username;
    }

    public void SetGold(int gold)
    {
        this.gold.text = gold.ToString();
    }

   
}