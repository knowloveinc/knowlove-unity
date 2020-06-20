using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


//This script is only required to get it to show the custom variables that were added instead of displaying the inspector for the base ScrollRect
namespace GameBrewStudios
{
    [CustomEditor(typeof(RefreshableScrollRect), true)]
    [CanEditMultipleObjects]
    public class RefreshableScrollRectInspector : Editor
    {

        //Unity's built-in editor
        Editor defaultEditor;
        RefreshableScrollRect scrollRect;

        void OnEnable()
        {
            scrollRect = target as RefreshableScrollRect;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    } 
}