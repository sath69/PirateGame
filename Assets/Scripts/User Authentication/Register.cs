using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using System.Net.Mail;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class Register : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text registerWarningText;
    public Button registerButton;
    
    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseUser User;
    private DependencyStatus dependencyStatus;


    //Checks whether the Authentication System is activated in the Unity Project.
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
    }

    //Intialises the Authentication for Firebase.
    private void IntialiseFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }
    
    //Detects whether the user has clicked the register button.
    public void OnRegisterBtnClicked()
    {
        //If statements to enforce the validation before the user can create their account.
        var(msg, result) = PasswordStrength(passwordField.text, usernameField.text, emailField.text);
        if (IsEmpty(usernameField.text, emailField.text, passwordField.text))
        {
            registerWarningText.text = "Please fill in all the fields!";
            registerButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        else if (!result && (!IsValidEmailFormat(emailField.text) || !IsValidEmailDomain(emailField.text)) && usernameField.text.Length < 3)
        {
            registerWarningText.text = "Registration details are invalid!";
            registerButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        if (usernameField.text.Length < 3)
        {
            registerWarningText.text = "Username is too short!";
            registerButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        if (!result)
        {
            registerWarningText.text = msg;
            registerButton.enabled = false;
            Invoke(nameof(ClearTextAfterSeconds),5f);
            return;
        }
        else if (!IsValidEmailFormat(emailField.text) || !IsValidEmailDomain(emailField.text))
        {
           registerWarningText.text = "Email is not valid!";
           registerButton.enabled = false;
           Invoke(nameof(ClearTextAfterSeconds),5f);
           return;
        }
        else
        {
            StartCoroutine(RegisterUser(emailField.text, passwordField.text, usernameField.text));
        }
    }

    //Function to add the register details to the Authentication database.
    private IEnumerator RegisterUser(string _email, string _password, string _username)
    {
        //Attempts to create a new user through Firebase.
        var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);
        
        if(RegisterTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to perform register task {RegisterTask.Exception}");
            FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            //Error handlers if register details weren't typed up to standard.
            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                     registerWarningText.text = "Email has already been used!";
                     registerButton.enabled = false;
                     Invoke(nameof(ClearTextAfterSeconds),5f);
                     break;
                case AuthError.AccountExistsWithDifferentCredentials:
                     registerWarningText.text = "Account already exists!";
                     registerButton.enabled = false;
                     Invoke(nameof(ClearTextAfterSeconds),5f);
                     break;   
            }
        }
        else
        { 
            //Sets the username for the user's account.
            User = RegisterTask.Result.User;
            if(User != null)
            {
                UserProfile profile = new UserProfile {DisplayName = _username};
                var ProfileTask = User.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
                Debug.Log("Updating Username.");

                //If assigning a  username doesn't work.
                if(ProfileTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to perform register task {RegisterTask.Exception}");
                    FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    Debug.LogError(errorCode);
                }
                else
                { 
                   //Text to display that registration is successful. Text gets cleared up after 8 seconds.
                   registerWarningText.color = Color.green;
                   registerWarningText.text ="Registration is successful!";
                   Invoke(nameof(ClearTextAfterSeconds),8f);
                   registerButton.enabled = true;
                   usernameField.text = "";
                   passwordField.text = "";
                   emailField.text = "";
                }

            }
        }
    }

    //Function that clears up the register status text and sets the color back to default (red).
    private void ClearTextAfterSeconds()
    {
        registerButton.enabled = true;
        registerWarningText.text = "";
        registerWarningText.color = Color.red;
    }
   
    //Checks whether any of the input fields are empty if user attempts to press "Submit"
    private bool IsEmpty(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return true;
        }
        return false;
    }

    //Checks the password inputted by user is strong
    private static (string msg, bool isStrong) PasswordStrength(string password, string username, string email)
    {
        //Checks whether the username substring is contained within the password. 
        if(password.ToLower().Contains(username.ToLower()))
        {
            return ("Password cannot contain your username!",false);
        }
        bool isAlphaNumeric = true;
        foreach(char c in password)
        {
            if(!char.IsLetterOrDigit(c))
            {
               isAlphaNumeric = false;
            }
        }
        if (password.Length < 8)
        {
            return("Password must at least be 8 characters!", false);
        }
        if (isAlphaNumeric)
        {
            return("Password must at least contain one special character!", false);
        }
        return ("Password is strong!", true);
    }
    
    //Checks if email is valid. Uses the library for checking whether an email follows the standard convention.
    private bool IsValidEmailFormat(string email)
    {
        try
        {
            var result = new MailAddress(email);
            return result.Address == email;
        }
        catch
        {
            return false;
        }

    }

    //Uses the substring of the user's email at the part where it begins at '@' and uses sockets to check whether the email belongs in a valid domain.
    private bool IsValidEmailDomain(string email)
    {
        try
        {
            string domain = email.Substring(email.IndexOf('@')+ 1);
            IPHostEntry host = Dns.GetHostEntry(domain);
            return host.AddressList.Length > 0;
        }
        catch(SocketException)
        {
            return false;
        }
    }
}