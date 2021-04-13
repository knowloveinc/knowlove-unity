using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Knowlove.UI
{
    public class PopupDialog : MonoBehaviour
    {
        public static PopupDialog Instance;

        private static List<PopupDialogMessage> messages = new List<PopupDialogMessage>();

        public enum PopupButtonColor
        {
            Plain,
            Green,
            Red
        }

        public struct PopupButton
        {
            public string text;
            public PopupButtonColor buttonColor;
            public System.Action onClicked;
        }

        public struct PopupDialogMessage
        {
            public string title;
            public string body;
            public PopupButton[] buttons;
        }

        public CanvasGroup canvasGroup;

        public PopupDialogWindow dialogWindow;

        private float stuckTime = 0f;

        public static bool isShowing
        {
            get => Instance.dialogWindow.gameObject.activeSelf;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);

            PopupDialogWindow.OnPopupDialogClosed += this.OnPopupDialogClosed;
        }

        private void Update()
        {
            if (!this.dialogWindow.gameObject.activeSelf && canvasGroup.alpha > 0f)
                stuckTime += Time.deltaTime;

            if (stuckTime > 1f)
            {
                Close();
                stuckTime = 0f;
            }
        }

        private void OnPopupDialogClosed()
        {
            if (messages != null && messages.Count > 0)
            {
                PopupDialogMessage message = messages[0];
                messages.RemoveAt(0);
                ShowMessage(message);
            }
            else
                Close();
        }
        public void Close()
        {
            this.canvasGroup.alpha = 0f;
            this.canvasGroup.interactable = false;
            this.canvasGroup.blocksRaycasts = false;
        }

        public void ToggleCanvasGroup(bool active)
        {
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;

            if (active)
                canvasGroup.DOFade(1f, 0.25f);
            else
                canvasGroup.DOFade(0f, 0.25f);
        }

        public void Show(string body)
        {
            Show("", body);
        }

        public void Show(string title, string body)
        {
            Show(title, body, null);
        }

        public void Show(string title, string body, int bgColor)
        {
            Show(title, body, null, bgColor);
        }

        public void Show(string title, string body, PopupButton[] buttons, int bgColor = 0, bool showButtons = true)
        {
            if (buttons == null)
            {
                buttons = new PopupButton[]
                {
                    new PopupButton() { text = "Okay", buttonColor = PopupButtonColor.Plain }
                };
            }

            PopupDialogMessage pdm = new PopupDialogMessage() { title = title, body = body, buttons = showButtons ? buttons : null };

            //if(isShowing)
            //{
            //    Debug.Log("Queing message because one is already showing.");
            //    QueueMessage(pdm);
            //    return;
            //}

            ShowMessage(pdm, bgColor);
        }


        private void QueueMessage(PopupDialogMessage message)
        {
            messages.Add(message);
        }

        private void ShowMessage(PopupDialogMessage message, int bgColor = 0)
        {
            this.canvasGroup.alpha = 1f;
            this.canvasGroup.interactable = true;
            this.canvasGroup.blocksRaycasts = true;
            dialogWindow.SetMessage(message, bgColor);
            dialogWindow.Show();
        }
    }
}
