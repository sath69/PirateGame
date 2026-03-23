using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UIComponent
{
    LoggedInUI,
    NotLoggedInUI,
    Login,
    Play,
    Register,
    Create,
    Join,
    Find
}

public class Menu_UIManager : MonoBehaviour
{
    public Button MplayButton;
    public Button MloginButton;
    public Button MregisterButton;
    public Button McreateGameButton;
    public Button MjoinGameButton;
    public Button MfindGameButton;
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject playUI;
    public GameObject notLoggedInUI;
    public GameObject loggedInUI;
    public GameObject createGameUI;
    public GameObject joinGameUI;
    public GameObject findGameUI;
    public UIComponent currentUI;
    public TMP_Text signedInAsText;
    public static Menu_UIManager Instance {get; set;}

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        notLoggedInUI.SetActive(true);
        currentUI = UIComponent.NotLoggedInUI;
    }

    public void OnPlayBtnClicked()
    {
        loggedInUI.SetActive(false);
        playUI.SetActive(true);
        currentUI = UIComponent.Play;
    }
    
    public void OnRegisterBtnClicked()
    {
        notLoggedInUI.SetActive(false);
        registerUI.SetActive(true);
        currentUI = UIComponent.Register;
    }

    public void OnLoginBtnClicked()
    {
        notLoggedInUI.SetActive(false);
        loginUI.SetActive(true);
        currentUI = UIComponent.Login;
    }
    public void OnCreateGameBtnClicked()
    {
        playUI.SetActive(false);
        createGameUI.SetActive(true);
        currentUI = UIComponent.Create;
    }

    public void OnJoinGameBtnClicked()
    {
        playUI.SetActive(false);
        joinGameUI.SetActive(true);
        currentUI = UIComponent.Join;
    }

    public void OnFindGameBtnClicked()
    {
        playUI.SetActive(false);
        findGameUI.SetActive(true);
        currentUI = UIComponent.Find;
    }
    public void OnLoginSuccess(string User)
    {
        notLoggedInUI.SetActive(false);
        loginUI.SetActive(false);
        loggedInUI.SetActive(true);
        signedInAsText.enabled = true;
        signedInAsText.text = $"Signed in as {User}";
        currentUI = UIComponent.LoggedInUI;
    }
    public void OnLogOutButtonClicked()
    {
        loggedInUI.SetActive(false);
        notLoggedInUI.SetActive(true);
        currentUI = UIComponent.NotLoggedInUI;
        Login.Instance.Logout();
        signedInAsText.enabled = false;
        signedInAsText.text = "";
        Debug.Log("Signed out!");
    }

    public void OnBackBtnClicked()
    {
        switch (currentUI)
        {
            case UIComponent.NotLoggedInUI:
               return;
            case UIComponent.LoggedInUI:
               return;
            case UIComponent.Login:
               loginUI.SetActive(false);
               notLoggedInUI.SetActive(true);
               currentUI = UIComponent.NotLoggedInUI;
               return;
            case UIComponent.Register:
               registerUI.SetActive(false);
               notLoggedInUI.SetActive(true);
               currentUI = UIComponent.NotLoggedInUI;
               return;
            case UIComponent.Play:
               playUI.SetActive(false);
               loggedInUI.SetActive(true);
               currentUI = UIComponent.LoggedInUI;
               return;
            case UIComponent.Create:
               createGameUI.SetActive(false);
               playUI.SetActive(true);
               currentUI = UIComponent.Play;
               return;
            case UIComponent.Join:
               joinGameUI.SetActive(false);
               playUI.SetActive(true);
               currentUI = UIComponent.Play;
               return;
            case UIComponent.Find:
               findGameUI.SetActive(false);
               playUI.SetActive(true);
               currentUI = UIComponent.Play;
               return;
        }
    }

    public void EnableBackButton(Button backButton)
    {
        backButton.enabled = true;
    }
    
    public void DisableBackButton(Button backButton)
    {
        backButton.enabled = false;
    }
}