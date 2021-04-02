using DG.Tweening;
using GameBrewStudios;
using GameBrewStudios.Networking;
using Newtonsoft.Json;
using Photon.Pun;

using TMPro;

using UnityEngine;

namespace Knowlove.UI.Menus
{
    public class Window_Login : Window
    {
        [SerializeField]
        TMP_InputField loginEmailField, loginPasswordField, registerEmailField, registerPasswordField1, registerPasswordField2, registerNicknameField;

        [SerializeField]
        Window_MatchList matchListWindow;

        [SerializeField]
        Window_PickMode pickModeWindow;

        [SerializeField]
        RectTransform loginBox, registerBox;

        private const string USERNAME_KEY = "SavedUsername", PASSWORD_KEY = "SavedPassw";

        private void Awake()
        {
            registerBox.anchoredPosition = new Vector2(registerBox.anchoredPosition.x, 2000f);
            loginBox.anchoredPosition = new Vector2(loginBox.anchoredPosition.x, 0f);
        }

        public void CheckForConnection()
        {

            CanvasLoading.Instance.Show();
            APIManager.Connect((success) =>
            {
                CanvasLoading.Instance.Hide();
                if (success)
                {
                    loginEmailField.SetTextWithoutNotify(PlayerPrefs.GetString(USERNAME_KEY, ""));
                    loginPasswordField.SetTextWithoutNotify(PlayerPrefs.GetString(PASSWORD_KEY, ""));
                }
                else
                {
                    PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                    {
                    new PopupDialog.PopupButton()
                    {
                        text = "Try again",
                        onClicked = () =>
                        {
                            CheckForConnection();
                        }
                    }
                    };
                    PopupDialog.Instance.Show("No Internet Connection", "Unable to connect to authentication servers. Please check your internet connection and try again.", buttons);
                }
            });
        }

        public override void Show()
        {
            base.Show();


            loginEmailField.text = "";
            loginPasswordField.text = "";
            registerEmailField.text = "";
            registerPasswordField1.text = "";
            registerPasswordField2.text = "";
            registerNicknameField.text = "";

            Debug.Log("Showing login window...");
            CheckForConnection();
        }

        bool isAuthenticating = false;

        public void DoLogin()
        {
            Debug.Log("Running DoLogin...");
            if (isAuthenticating) return;


            isAuthenticating = true;

            string emailError;
            string passwordError;

            bool emailIsValid = Validation.ValidateEmail(loginEmailField.text, out emailError);
            bool passwordIsValid = Validation.ValidatePassword(loginPasswordField.text, out passwordError);
            if (emailIsValid && passwordIsValid)
            {

                Debug.Log("Starting api call for authentication...");
                ServerAPI.OnError += this.ServerAPI_OnError;

                CanvasLoading.Instance.Show();
                APIManager.Authenticate(loginEmailField.text, loginPasswordField.text, (success) =>
                {
                    CanvasLoading.Instance.Hide();

                    Debug.LogWarning("FINISHED WITH RESULT: " + success);

                    //On successful login, show the Match Finder
                    if (success)
                    {
                        PlayerPrefs.SetString(USERNAME_KEY, loginEmailField.text);
                        PlayerPrefs.SetString(PASSWORD_KEY, loginPasswordField.text);

                        //User logged in, what next?
                        //PopupDialog.Instance.Show("Logged in successfully.");
                        this.Hide();
                        pickModeWindow.Show();
                    }
                    else
                    {
                        ShowErrorMessage();
                    }

                    ServerAPI.OnError -= this.ServerAPI_OnError;
                    isAuthenticating = false;
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(emailError) || !string.IsNullOrEmpty(passwordError))
                {
                    Debug.LogError(emailError + " " + passwordError);
                    PopupDialog.Instance.Show("Invalid Username or Password", emailError + " " + passwordError);
                }

                isAuthenticating = false;
            }


        }

        public void DoRegister()
        {
            Debug.Log("Running DoLogin...");
            if (isAuthenticating) return;


            isAuthenticating = true;

            string emailError;
            string passwordError;

            bool emailIsValid = Validation.ValidateEmail(registerEmailField.text, out emailError);
            bool passwordIsValid = Validation.ValidatePassword(registerPasswordField1.text, out passwordError) && registerPasswordField1.text == registerPasswordField2.text;
            if (emailIsValid && passwordIsValid)
            {

                Debug.Log("Starting api call for authentication...");
                ServerAPI.OnError += this.ServerAPI_OnError;

                CanvasLoading.Instance.Show();
                APIManager.Register(registerEmailField.text, registerNicknameField.text, registerPasswordField1.text, (success) =>
                {
                    CanvasLoading.Instance.Hide();

                    Debug.LogWarning("FINISHED WITH RESULT: " + success);

                    //On successful login, show the Match Finder
                    if (success)
                    {
                        //User logged in, what next?
                        //PopupDialog.Instance.Show("Logged in successfully.");
                        PhotonNetwork.NickName = User.current.displayName;
                        Debug.Log("<color=Magenta>User nickname set to: " + PhotonNetwork.NickName + "</color>");
                        this.Hide();
                        pickModeWindow.Show();
                    }
                    else
                    {
                        ShowErrorMessage();
                    }

                    ServerAPI.OnError -= this.ServerAPI_OnError;
                    isAuthenticating = false;
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(emailError) || !string.IsNullOrEmpty(passwordError))
                {
                    Debug.LogError(emailError + " " + passwordError);
                    PopupDialog.Instance.Show("Invalid Username or Password", emailError + " " + passwordError);
                }
                else if (registerPasswordField1.text != registerPasswordField2.text)
                {
                    Debug.LogError("Passwords do not match.");
                    PopupDialog.Instance.Show("Invalid Username or Password", "Passwords do not match.");
                }
                else
                {
                    Debug.LogError("WHY ARE WE HERE???");
                }

                isAuthenticating = false;
            }


        }

        private ServerAPI.ServerError lastError;

        public void ShowLoginBox()
        {
            registerBox.DOAnchorPosY(2000f, 0.5f);
            loginBox.anchoredPosition = new Vector2(loginBox.anchoredPosition.x, -2000f);
            loginBox.DOAnchorPosY(0f, 0.5f);
        }

        public void ShowRegisterBox()
        {
            loginBox.DOAnchorPosY(2000f, 0.5f);
            registerBox.anchoredPosition = new Vector2(registerBox.anchoredPosition.x, -2000f);
            registerBox.DOAnchorPosY(0f, 0.5f);
        }

        private void ServerAPI_OnError(ServerAPI.ServerError obj)
        {
            lastError = obj;
        }

        [SerializeField]
        Window_ResetPassword resetPasswordWindow;

        public void ForgotPassword()
        {
            bool emailIsValid = Validation.ValidateEmail(loginEmailField.text, out string emailError);

            if (emailIsValid)
            {
                ServerAPI.OnError += this.ServerAPI_OnError;
                PopupDialog.PopupButton[] buttons = new PopupDialog.PopupButton[]
                {
                new PopupDialog.PopupButton()
                {
                    text = "Yes",
                    onClicked = () =>
                    {
                        CanvasLoading.Instance.Show();

                        APIManager.ForgotPassword(loginEmailField.text.Trim(), (result) =>
                        {
                            ServerAPI.OnError -= this.ServerAPI_OnError;
                            CanvasLoading.Instance.Hide();

                            Debug.Log(JsonConvert.SerializeObject(result));

                            if(result != null && result.ContainsKey("success") && (bool)result["success"] == true)
                            {
                                Debug.Log("Showing reset password screen...");
                                resetPasswordWindow.Show(loginEmailField.text.Trim());
                                this.Hide();
                            }
                            else
                            {
                                PopupDialog.Instance.Show("Something went wrong while processing your request. Please try again.\n" + (!string.IsNullOrEmpty(lastError.text) ? lastError.text : "") );
                            }
                        });
                    }
                },
                new PopupDialog.PopupButton()
                {
                    text = "No",
                    buttonColor = PopupDialog.PopupButtonColor.Plain
                }
                };

                PopupDialog.Instance.Show("Forgot Password", "Are you sure you want to reset your password? Click YES to receive an email with a 6-digit code that you can enter on the next screen to reset your password.", buttons);
            }
            else
            {
                PopupDialog.Instance.Show("Invalid Email", emailError + "\n\n Please try again.");
            }
        }


        void ShowErrorMessage()
        {
            Debug.LogError("Showing login error...  " + lastError.text);

            //if (!string.IsNullOrEmpty(lastError.text))
            //{


            PopupDialog.Instance.Show("Login Error", lastError.ToString());
            //}
            //else
            //{
            //    PopupDialog.Instance.Show("Something went wrong. Please check your connection and try again.");
            //}
        }
    }
}

