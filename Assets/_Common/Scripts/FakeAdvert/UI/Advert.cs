using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.FakeAdvert.UI {

	[DisallowMultipleComponent()]
	public class Advert : MonoBehaviour {

		public delegate void CloseDelegate(float openTime);
		public event CloseDelegate OnClose;

		protected float timeStamp;
		protected float TimeSinceOpen => Time.realtimeSinceStartup - timeStamp;

		public enum PlacementType
		{
			Fullscreen,
			SideBar
		}

		[SerializeField] protected PlacementType _placement = PlacementType.Fullscreen;
		public PlacementType Placement => _placement;

		[SerializeField] protected Button closeBtn;

		public virtual void Open()
		{
			Debug.Log("[" + nameof(Advert) + "] Open ad "+gameObject.name);

			timeStamp = Time.realtimeSinceStartup;
			gameObject.SetActive(true);

			RegisterEvents();
		}

		public virtual void Close()
		{
			Debug.Log("[" + nameof(Advert) + "] Close ad " + gameObject.name + " after: "+TimeSinceOpen+"s");

			OnClose?.Invoke(TimeSinceOpen);
			gameObject.SetActive(false);
			UnRegisterEvents();
		}

		/// <summary>
		/// Register your event on Open
		/// </summary>
		protected virtual void RegisterEvents() 
		{
			closeBtn.onClick.AddListener(Close);
		}

		/// <summary>
		/// Register your event on Close
		/// </summary>
		protected virtual void UnRegisterEvents() 
		{
			OnClose = null;
			closeBtn.onClick.RemoveListener(Close);
			StopAllCoroutines();
		}
	}
}