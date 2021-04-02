using GameBrewStudios;
using GameBrewStudios.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Knowlove.UI.Menus
{
    public class Window_ListEditor : Window
    {
        [SerializeField]
        GameObject prefab, leftLoading, rightLoading;

        [SerializeField]
        Transform leftContainer, rightContainer;

        public override void Show()
        {
            base.Show();
            PopulateLeftSide();
            PopulateRightSide();
        }

        public override void Hide()
        {
            base.Hide();
        }


        public void AddToList(string text)
        {
            if (User.current.nonNegotiableList.Contains(text)) return;

            if (User.current.nonNegotiableList.Count < 10)
            {
                User.current.nonNegotiableList.Add(text);

                CanvasLoading.Instance.Show();
                APIManager.UpdateNonNegotiableList(User.current.nonNegotiableList, (response) =>
                {
                    Debug.Log("UpdateNonNegotiableListResponse success = " + response.success);
                    CanvasLoading.Instance.Hide();
                    if (response != null && response.success)
                    {
                        User.current.nonNegotiableList = response.list;
                        PopulateLeftSide();
                        PopulateRightSide();
                    }
                    else
                    {
                        PopupDialog.Instance.Show("Something went wrong.");
                    }
                });
            }
            else
            {
                PopupDialog.Instance.Show("Sorry! You already have 10 items on your list. You can remove items from your list by tapping on them.");
            }
        }

        public List<string> listItems;

        public void PopulateLeftSide()
        {
            leftLoading.SetActive(true);
            if (listItems == null || listItems.Count == 0)
            {
                TextAsset ta = Resources.Load<TextAsset>("NonNegotiableListItems");
                listItems = JsonConvert.DeserializeObject<List<string>>(ta.text);
            }

            foreach (Transform child in leftContainer)
            {
                if (child.gameObject.activeSelf)
                    Destroy(child.gameObject);
            }

            foreach (string item in listItems)
            {
                GameObject obj = Instantiate(prefab, leftContainer, false);
                TextMeshProUGUI label = obj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                label.text = item;

                Button btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                if (User.current.nonNegotiableList != null && User.current.nonNegotiableList.Count > 0 && User.current.nonNegotiableList.Contains(item))
                {
                    btn.interactable = false;
                }
                else
                {
                    btn.interactable = true;
                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log("Clicked on: " + item);
                        AddToList(item);
                    });
                }
            }

            leftLoading.SetActive(false);
        }

        public void PopulateRightSide()
        {
            rightLoading.SetActive(true);
            foreach (Transform child in rightContainer)
            {
                if (child.gameObject.activeSelf)
                    Destroy(child.gameObject);
            }

            foreach (string item in User.current.nonNegotiableList)
            {
                GameObject obj = Instantiate(prefab, rightContainer, false);
                TextMeshProUGUI label = obj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                label.text = item;

                Button btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PopupDialog.PopupButton[] btns = new PopupDialog.PopupButton[]
                    {
                    new PopupDialog.PopupButton()
                    {
                        text = "Yes, Remove it",
                        buttonColor = PopupDialog.PopupButtonColor.Red,
                        onClicked = () =>
                        {
                            User.current.nonNegotiableList.Remove(item);

                            CanvasLoading.Instance.Show();
                            APIManager.UpdateNonNegotiableList(User.current.nonNegotiableList, (response) =>
                            {
                                Debug.Log("UpdateNonNegotiableListResponse success = " + response.success);
                                CanvasLoading.Instance.Hide();
                                if(response != null && response.success)
                                {
                                    User.current.nonNegotiableList = response.list;
                                    PopulateLeftSide();
                                    PopulateRightSide();
                                }
                                else
                                {
                                    PopupDialog.Instance.Show("Something went wrong.");
                                }
                            });
                        }
                    },
                    new PopupDialog.PopupButton()
                    {
                        text = "Nevermind",
                        buttonColor = PopupDialog.PopupButtonColor.Plain,
                        onClicked = () =>
                        {

                        }
                    }
                    };
                    PopupDialog.Instance.Show("", "Remove this item from your Non-Negotiable List?\n\"" + item + "\"", btns);
                });
            }

            rightLoading.SetActive(false);
        }

    }
}

