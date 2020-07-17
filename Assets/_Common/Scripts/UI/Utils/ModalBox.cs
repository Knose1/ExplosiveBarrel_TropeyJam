///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 13/05/2020 01:29
///-----------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.UI.Utils {
	[RequireComponent(typeof(RectTransform))]
	/// <summary>
	/// A class used to display modalbox
	/// </summary>
	/// <example>
	/// IEnumerator MyModalBoxCoroutine()
	///		//Let's Create the modal box
	///		ModalBox box = Instantiate(modalBoxPrefab, canvas);
	///		
	///		box.SetTitle("Example");
	///		box.SetMessage("This is a {0} example","stoopid");
	///		
	///		box.AddButton("Cancel", out ModalButton cancel);
	///		box.AddButton("Confirm", out ModalButton confirm);
	///		
	///		//Wait for player input on the modal box
	///		yield return box.Show();
	///		
	///		//If the player clicked on confirm
	///		if (box.ClickedButton == confirm)
	///		{
	///			Debug.log("Confirm");
	///		}
	///		else if (box.ClickedButton == cancel)
	///		{
	///			Debug.log("Cancel");
	///		}
	///	}
	/// </example>
	public class ModalBox : MonoBehaviour {

		[Header("Prefab")]
		[SerializeField, Tooltip("A button with text that we can instatiate in the buttonContainer")] protected ModalButton buttonPrefab = null;

		[Header("Dynamic Content")]
		[SerializeField, Tooltip("Where we can instatiate buttonPrefab(s)")] protected RectTransform buttonContainer = null;
		[SerializeField, Tooltip("Root of the modalbox, it'll be disactivated in the awake")] protected GameObject root = null;
		[SerializeField, Tooltip("The title area of the modalbox")] protected Text title = null;
		[SerializeField, Tooltip("The message area of the modalbox")] protected Text message = null;

		/// <summary>
		/// A list of instantied buttons
		/// </summary>
		protected List<ModalButton> buttons = new List<ModalButton>();

		/// <summary>
		/// Whenever a player has clicked or not on a button
		/// </summary>
		private bool hasClicked = false;

		private ModalButton _clickedButton;

		/// <summary>
		/// Which button was clicked (there can be only one)
		/// </summary>
		public ModalButton ClickedButton => _clickedButton;

		private void Awake()
		{
			root.SetActive(false);
		}

		/// <summary>
		/// Set the title of the modalbox
		/// </summary>
		/// <param name="title"></param>
		public void SetTitle(string title)
		{
			this.title.text = title;
		}

		/// <summary>
		/// Set the content (message) of the modalbox
		/// </summary>
		/// <param name="message"></param>
		public void SetMessage(string message)
		{
			this.message.text = message;
		}

		/// <summary>
		/// Set the content (message) of the modalbox
		/// If the message has a certain format, you can pass the args as if you were doing string.Format(message, arg1, arg2 ...)
		/// </summary>
		/// <param name="message">An id for the localisation manager</param>
		/// <param name="args">Args to be</param>
		public void SetMessage(string message, params object[] args) {
			SetMessage(message);
			FormatMessage(args);
		}

		/// <summary>
		/// Set the args of the format.<br/>
		/// <br/>
		/// Example: <br/>
		/// SetMessage("Hi {0}.");<br/>
		/// FormatMessage("cute user");<br/>
		/// </summary>
		/// <param name="args"></param>
		public void FormatMessage(params object[] args)
		{
			message.text = string.Format(message.text, args);
		}

		/// <summary>
		/// Add a button with a specific name
		/// <param name="name"></param>
		/// <param name="button"></param>
		public void AddButton(string name, out ModalButton button)
		{
			button = Instantiate(buttonPrefab, buttonContainer);

			ModalButton refButton = button;
			button.SetText(name);
			button.transform.localScale = Vector3.one;

			button.OnClick.AddListener(() => {
				//Anonyme function to use refButton var
				_clickedButton = refButton;
				hasClicked = true;
			});

			buttons.Add(button);
		}


		/// <summary>
		/// Remove a button from the modalbox
		/// </summary>
		/// <param name="button"></param>
		public void RemoveButton(ModalButton button)
		{
			button.OnClick.RemoveAllListeners();
			button.transform.SetParent(null);
			buttons.Remove(button);
			
			Destroy(button);
		}

		/// <summary>
		/// Activates the modalbox and wait for a button to be clicked.
		/// You must use it in a coroutine;
		/// Don't forget, the modalBox is not automatically closed
		/// </summary>
		/// <returns></returns>
		public IEnumerator Show()
		{
			_clickedButton = null;
			hasClicked = false;
			root.SetActive(true);
			yield return new WaitUntil(PredicateHasClicked);
		}

		/// <summary>
		/// Activates the modalbox and wait for a button to be clicked.
		/// You can specify onEnd to know when someone clicked on the modalbox
		/// </summary>
		public void Show(Action<ModalBox> onEnd)
		{
			root.SetActive(true);
			StartCoroutine(ShowCoroutine(onEnd));
		}

		private IEnumerator ShowCoroutine(Action<ModalBox> onEnd) 
		{
			yield return new WaitUntil(PredicateHasClicked);
			onEnd?.Invoke(this);
		}

		private bool PredicateHasClicked()
		{
			return hasClicked;
		}


		/// <summary>
		/// Creates a modalbox with only one button (the button "Ok").
		/// Don't forget to call ShowSimpleAlert
		/// </summary>
		/// <param name="modalBoxPrefab"></param>
		/// <param name="parent"></param>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="buttonName"></param>
		/// <returns></returns>
		static public ModalBox CreateSimpleAlert(ModalBox modalBoxPrefab, Transform parent, string message, string title = "Alert", string buttonName = "Ok")
		{
			ModalBox modalBox = Instantiate(modalBoxPrefab, parent);

			modalBox.SetTitle(title);
			modalBox.SetMessage(message);

			modalBox.AddButton(buttonName, out ModalButton ok);

			return modalBox;
		}
		/// <summary>
		/// Show the modalbox.
		/// The modalbox is closed when clicking on the Ok button.
		/// </summary>
		/// <param name="modalBox"></param>
		/// <returns></returns>
		static public IEnumerator ShowSimpleAlert(ModalBox modalBox)
		{
			yield return modalBox.Show();

			Destroy(modalBox.gameObject);
		}
	}
}