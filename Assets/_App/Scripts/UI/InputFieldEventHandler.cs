using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Knowlove.UI
{
    public class InputFieldEventHandler : MonoBehaviour, IPointerClickHandler
    {

        public bool checkForTab = false;
        public GameObject selectOnTabKeyPressed;

        public bool checkForEnter = false;
        public UnityEvent OnSubmit;

        private static InputFieldEventHandler current;

        private void Awake()
        {
        }

        public void Update()
        {
            if (PopupDialog.isShowing) return;

            if (EventSystem.current.currentSelectedGameObject == this.gameObject && (current == null || current == this))
            {

                //Debug.Log("THIS IS ACTIVE: " + gameObject.name);

                if (checkForTab == true && Input.GetKeyDown(KeyCode.Tab))
                {
                    StartCoroutine(SetCurrentInput());
                    return;
                }

                if (checkForEnter == true && Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                {
                    if (OnSubmit != null) OnSubmit.Invoke();
                }

            }
        }



        IEnumerator SetCurrentInput()
        {
            yield return new WaitForEndOfFrame();
            current = selectOnTabKeyPressed.GetComponent<InputFieldEventHandler>();
            EventSystem.current.SetSelectedGameObject(selectOnTabKeyPressed);
        }

        public void OnEndEdit(string val)
        {
            Debug.Log("Editing ended on: " + gameObject.name);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            current = this;
        }
    }
}

