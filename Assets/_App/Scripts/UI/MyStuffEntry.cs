using DG.Tweening;
using Knowlove.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI
{
    public class MyStuffEntry : MonoBehaviour
    {
        Window_MyStuff myStuffWindow;

        [SerializeField]
        TextMeshProUGUI nameLabel, countLabel;

        [SerializeField]
        Image icon;

        [SerializeField]
        InventoryItemIconDefinition item;

        Button btn;

        public void Init(InventoryItemIconDefinition item, int count, Window_MyStuff myStuffWindow)
        {
            this.myStuffWindow = myStuffWindow;
            this.item = item;

            icon.sprite = this.item.icon;

            nameLabel.text = item.displayName;
            countLabel.text = item.hideStackCount ? "" : "<size=20>x</size><b>" + count.ToString("00") + "</b>";

            btn = GetComponent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    OnClicked(this);
                });
            }
            else
                Debug.LogError("Button component not found.");
        }

        public void OnClicked(MyStuffEntry entry)
        {
            entry.gameObject.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f, 0, 0.5f);
            Debug.Log("Clicked on Inventory Item in MyStuff");

            switch (entry.item.id.ToUpperInvariant())
            {
                case "BROWSECARDS":
                    PopupDialog.PopupButton[] a = new PopupDialog.PopupButton[]
                    {
                    new PopupDialog.PopupButton()
                    {
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        text = "Dating Deck",
                        onClicked = () =>
                        {
                            DeckBrowser.Instance.Show("dating");
                        }
                    },
                    new PopupDialog.PopupButton()
                    {
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        text = "Relationship Deck",
                        onClicked = () =>
                        {
                            DeckBrowser.Instance.Show("relationship");
                        }
                    },
                    new PopupDialog.PopupButton()
                    {
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        text = "Marriage Deck",
                        onClicked = () =>
                        {
                            DeckBrowser.Instance.Show("marriage");
                        }
                    }
                    };
                    PopupDialog.Instance.Show("Browse Cards", "Choose a deck of cards to browse.", a);
                    break;
                case "CUSTOMLIST":
                    PopupDialog.PopupButton[] b = new PopupDialog.PopupButton[]
                    {
                    new PopupDialog.PopupButton()
                    {
                        buttonColor = PopupDialog.PopupButtonColor.Green,
                        text = "Modify My List",
                        onClicked = () =>
                        {
                            this.myStuffWindow.OpenListEditor();
                        }
                    },
                    new PopupDialog.PopupButton()
                    {
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        text = "Nevermind",
                        onClicked = () =>
                        {

                        }
                    }
                    };
                    PopupDialog.Instance.Show("Custom Non-Negotiable List", "Would you like to edit your custom Non-Negotiable List now?", b);
                    break;
                default:
                    break;
            }
        }
    }
}

