using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField joinCode;
    public TMP_Text errorText;
    public Button joinButton;
    public Button leaveLobbyButton;

    public static JoinLobby Instance {get; set;}

    private void Awake() 
    {
        Instance = this;
    }
    
    public void OnJoinButtonClicked()
    {
        if (IsEmpty(joinCode.text))
        {
            errorText.text = "Please fill in all the fields!";
            joinButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        LobbyManager.Instance.JoinLobbyByCode(joinCode.text);
        joinCode.text = "";
    }

    private bool IsEmpty(string joinCodeText)
    {
        if (string.IsNullOrEmpty(joinCodeText))
        {
            return true;
        }
        return false;
    }

    private void ClearTextAfterSeconds()
    {
        joinButton.enabled = true;
        errorText.text = "";
    }
     
    public void ClearText()
    {
        Invoke(nameof(ClearTextAfterSeconds),5f);
    }
}
