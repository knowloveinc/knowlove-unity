using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Touch
{
	/// <summary>This component calls the OnFingerTap event when a finger taps the screen.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanFingerTap")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Tap")]
	public class LeanFingerTap : MonoBehaviour
	{
		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}
		[System.Serializable] public class Vector3Event : UnityEvent<Vector3> {}
		[System.Serializable] public class IntEvent : UnityEvent<int> {}

		/// <summary>Ignore fingers with StartedOverGui?</summary>
		public bool IgnoreStartedOverGui = true;

		/// <summary>Ignore fingers with OverGui?</summary>
		public bool IgnoreIsOverGui;

		/// <summary>Do nothing if this LeanSelectable isn't selected?</summary>
		public LeanSelectable RequiredSelectable;

		/// <summary>How many times must this finger tap before OnTap gets called?
		/// 0 = Every time (keep in mind OnTap will only be called once if you use this).</summary>
		public int RequiredTapCount = 0;

		/// <summary>How many times repeating must this finger tap before OnTap gets called?
		/// 0 = Every time (e.g. a setting of 2 means OnTap will get called when you tap 2 times, 4 times, 6, 8, 10, etc).</summary>
		public int RequiredTapInterval;

		/// <summary>Called on the first frame the conditions are met.
		/// LeanFinger = The finger that triggered the event.</summary>
		public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } } [FormerlySerializedAs("onTap")] [FormerlySerializedAs("OnTap")] [SerializeField] private LeanFingerEvent onFinger;

		/// <summary>Called on the first frame the conditions are met.
		/// Int = The finger tap count.</summary>
		public IntEvent OnCount { get { if (onCount == null) onCount = new IntEvent(); return onCount; } } [SerializeField] private IntEvent onCount;

		/// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
		public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

		/// <summary>Called on the first frame the conditions are met.
		/// Vector3 = Start point based on the ScreenDepth settings.</summary>
		public Vector3Event OnWorld { get { if (onWorld == null) onWorld = new Vector3Event(); return onWorld; } } [FormerlySerializedAs("onPosition")] [SerializeField] private Vector3Event onWorld;
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			RequiredSelectable = GetComponentInParent<LeanSelectable>();
		}
#endif
		protected virtual void Start()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponentInParent<LeanSelectable>();
			}
		}

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerTap += HandleFingerTap;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerTap -= HandleFingerTap;
		}

		private void HandleFingerTap(LeanFinger finger)
		{
			// Ignore?
			if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
			{
				Debug.LogError("1");
				return;
			}

			if (IgnoreIsOverGui == true && finger.IsOverGui == true)
			{
				Debug.LogError("2");
				return;
			}

			if (RequiredTapCount > 0 && finger.TapCount != RequiredTapCount)
			{
				Debug.LogError("3");
				return;
			}

			if (RequiredTapInterval > 0 && (finger.TapCount % RequiredTapInterval) != 0)
			{
				Debug.LogError("4");
				return;
			}

			if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
			{
				Debug.LogError("5");
				return;
			}

			if (onFinger != null)
			{
				
				onFinger.Invoke(finger);
			}

			if (onCount != null)
			{
				
				onCount.Invoke(finger.TapCount);
			}

			if (onWorld != null)
			{
				Vector3 position = ScreenDepth.Convert(finger.StartScreenPosition, gameObject);

				onWorld.Invoke(position);
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanFingerTap))]
	public class LeanFingerTap_Inspector : LeanInspector<LeanFingerTap>
	{
		private bool showUnusedEvents;

		protected override void DrawInspector()
		{
			Draw("IgnoreStartedOverGui", "Ignore fingers with StartedOverGui?");
			Draw("IgnoreIsOverGui", "Ignore fingers with OverGui?");
			Draw("RequiredSelectable", "Do nothing if this LeanSelectable isn't selected?");
			Draw("RequiredTapCount", "How many times must this finger tap before OnTap gets called?\n\n0 = Every time (keep in mind OnTap will only be called once if you use this).");
			Draw("RequiredTapInterval", "How many times repeating must this finger tap before OnTap gets called?\n\n0 = Every time (e.g. a setting of 2 means OnTap will get called when you tap 2 times, 4 times, 6, 8, 10, etc).");

			EditorGUILayout.Separator();

			bool usedA = Any(t => t.OnFinger.GetPersistentEventCount() > 0);
			bool usedB = Any(t => t.OnCount.GetPersistentEventCount() > 0);
			bool usedC = Any(t => t.OnWorld.GetPersistentEventCount() > 0);

			EditorGUI.BeginDisabledGroup(usedA && usedC);
				showUnusedEvents = EditorGUILayout.Foldout(showUnusedEvents, "Show Unused Events");
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Separator();

			if (usedA == true || showUnusedEvents == true)
			{
				Draw("onFinger");
			}

			if (usedB == true || showUnusedEvents == true)
			{
				Draw("onCount");
			}

			if (usedC == true || showUnusedEvents == true)
			{
				Draw("ScreenDepth");
				Draw("onWorld");
			}
		}
	}
}
#endif