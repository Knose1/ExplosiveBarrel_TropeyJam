using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com.Github.Knose1.Common.UI.Utils {

	public class ProgressBar : MonoBehaviour {

		[SerializeField] public RectTransform outerBar;
		[SerializeField] public RectTransform innerBar;
		[SerializeField, Tooltip("The progress between 0 and 1")] protected float _progress = 0;

		/// <summary>
		/// The progress between 0 and 1
		/// </summary>
		public float Progress 
		{
			get => _progress;
			set
			{
				_progress = Mathf.Clamp(value, 0, 1);
				if (!innerBar) 
				{
					Debug.LogWarning(nameof(innerBar) + " is not set");
					return;
				}
				innerBar.anchorMax = new Vector2(_progress, 1);
			}
		}

		protected virtual void Awake()
		{
			StartCoroutine(DouilleCoroutine());
		}

		private IEnumerator DouilleCoroutine()
		{
			yield return null;
			Progress = _progress;
		}

		#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (!isActiveAndEnabled) return;
			StartCoroutine(DouilleCoroutine());
		}
		#endif

		private void Start()
		{
			Progress = _progress;
		}

		#if UNITY_EDITOR
		[Obsolete("This function can ONLY be used by Unity to generate a new "+nameof(ProgressBar), false)]
		[MenuItem("GameObject/UI/Common/" + nameof(ProgressBar), false, 11)]
		private static void CreateKeyInputButton(MenuCommand menu)
		{
			Color textColor = new Color(50/255, 50/255, 50/255);

			//Root GameObject
			GameObject root = new GameObject(nameof(ProgressBar));
			root.layer = 5; //UI layer

			//Root Transform
			RectTransform rootTr = root.AddComponent<RectTransform>();

			//Other GameObject
			GameObject outerGm = new GameObject("Outer");
			GameObject innerGm = new GameObject("Inner");

			//Other Transform
			RectTransform outerTr = outerGm.AddComponent<RectTransform>();
			RectTransform innerTr = innerGm.AddComponent<RectTransform>();
			
			//Add scripts
			Image outerImg = outerGm.AddComponent<Image>();
			Image innerImg = innerGm.AddComponent<Image>();
			ProgressBar progress = root.AddComponent<ProgressBar>();

			//Hierarchie
			innerGm.transform.SetParent(outerTr);
			outerGm.transform.SetParent(rootTr);

			//Set image
			outerImg.color = Color.white;
			innerImg.color = Color.green;

			//Set progress bar
			progress.outerBar = outerTr;
			progress.innerBar = innerTr;
			progress.Progress = 0.5f;

			//Set transform size
			rootTr.sizeDelta = new Vector2(400, 50);
			innerTr.sizeDelta = outerTr.sizeDelta = Vector2.zero;
			innerTr.anchorMin = outerTr.anchorMin = Vector2.zero;
			innerTr.anchorMax = outerTr.anchorMax = Vector2.one;

			//Set the menu parent, ensure unique name, register undo and Higlight
			GameObjectUtility.SetParentAndAlign(root, menu.context as GameObject);
			GameObjectUtility.EnsureUniqueNameForSibling(root);
			Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
			Selection.activeObject = root;

			if (root.transform.parent)
				root.layer = root.transform.parent.gameObject.layer; //Set parent's layer if parent
		}
		#endif

		private void OnDisable()
		{
			StopAllCoroutines();
		}
		private void OnDestroy()
		{
			StopAllCoroutines();
		}
	}
}