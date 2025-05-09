﻿using GameBrewStudios.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Window_ResetPassword : Window
{
    [SerializeField]
    Window_Login loginWindow;

    [SerializeField]
    TMP_InputField codeField, newPasswordField, confirmPasswordField;

    public override void Show()
    {
        Debug.LogError("This window has its own Show method that takes a string arguement for user email.");
    }

    public string email = "";

    public void Show(string email)
    {
        codeField.SetTextWithoutNotify("");
        newPasswordField.SetTextWithoutNotify("");
        confirmPasswordField.SetTextWithoutNotify("");
        this.email = email;

        base.Show();
    }

    public void Submit()
    {
        bool passwordIsValid = Validation.ValidatePassword(newPasswordField.text, out string passwordError);
        if (passwordIsValid && newPasswordField.text == confirmPasswordField.text)
        {
            ServerAPI.OnError += this.ServerAPI_OnError;
            CanvasLoading.Instance.Show();
            APIManager.ResetPassword(email, codeField.text, newPasswordField.text, (result) => 
            {
                ServerAPI.OnError -= this.ServerAPI_OnError;
                CanvasLoading.Instance.Hide();


                if(result != null && result.ContainsKey("success") && (bool)result["success"] == true)
                {
                    this.Hide();
                    loginWindow.Show();
                    PopupDialog.Instance.Show("Your password was successfully reset. You can now login using your new password.");
                }
                else
                {
                    if (result.ContainsKey("error"))
                    {
                        PopupDialog.Instance.Show((string)result["error"]);
                    }
                    else
                    {
                        PopupDialog.Instance.Show("Something went wrong while processing your request. Please try again. \n" + (!string.IsNullOrEmpty(lastError.text) ? lastError.text : ""));
                    }
                }
                
            });
        }
        else
        {
            if(!passwordIsValid && !string.IsNullOrEmpty(passwordError))
            {
                PopupDialog.Instance.Show(passwordError);
            }
            else if(newPasswordField.text != confirmPasswordField.text )
            {
                PopupDialog.Instance.Show("Passwords do not match.");
            }
        }
   }

    ServerAPI.ServerError lastError;

    private void ServerAPI_OnError(ServerAPI.ServerError obj)
    {
        lastError = obj;
    }

    public void GoBack()
    {
        this.Hide();
        loginWindow.Show();
    }

    public override void Hide()
    {
        this.email = "";
        base.Hide();
    }
}
