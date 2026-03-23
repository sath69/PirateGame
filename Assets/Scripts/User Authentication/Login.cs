using TMPro;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text loginWarningText;
    public Button loginButton;

    [Header("Firebase")]
    private FirebaseUser User;
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;

    public static Login Instance {get; set;}


    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
           dependencyStatus = task.Result;
           if(dependencyStatus == DependencyStatus.Available)
            {
                IntialiseFirebase();
            }
        });

        Instance = this;
    }

    private void IntialiseFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void OnLoginBtnClicked()
    {
        if(IsEmpty(emailField.text, passwordField.text))
        {
            loginWarningText.text = "Please fill in all the fields!";
            loginButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        StartCoroutine(LoginUser(emailField.text, passwordField.text));
    }
    public void Logout()
    {
        auth.SignOut();
    }
    private IEnumerator LoginUser(string _email, string _password)
    {
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        
        if(LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to perform login task {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            loginWarningText.text = "Login failed!";
            loginButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
        }
        else
        {
            loginWarningText.color = Color.green;
            loginWarningText.text = "Logged in!";
            loginButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            User = LoginTask.Result.User;
            Menu_UIManager.Instance.OnLoginSuccess(User.DisplayName);
            emailField.text = "";
            passwordField.text = "";
        }
    }

    private void ClearTextAfterSeconds()
    {
        loginWarningText.color = Color.red;
        loginButton.enabled = true;
        loginWarningText.text = "";
    }

    private bool IsEmpty(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return true;
        }
        return false;
    }

    public string GetUsername()
    {
        return User.DisplayName;
    }
}