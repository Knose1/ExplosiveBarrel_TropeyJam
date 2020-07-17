using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.FakeAdvert.UI {
	public class RewardAdvert : Advert {

		public delegate void WinDelegate(float openTime);
		public event WinDelegate OnWin;

		[SerializeField] protected Text countdown;
		[SerializeField] public float challengeDuration;
		[SerializeField] public Button winButton;

		protected void OnValidate()
		{
			if (_placement != PlacementType.Fullscreen)
			{
				Debug.LogWarning(nameof(RewardAdvert) + "s can only be in Fullscreen");
			}
			_placement = PlacementType.Fullscreen;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();
			countdown.gameObject.SetActive(true);
			closeBtn.gameObject.SetActive(true);
			winButton.gameObject.SetActive(false);
			StartCoroutine(CoroutineClaimReward());
		}

		protected IEnumerator CoroutineClaimReward()
		{
			yield return new WaitForSeconds(challengeDuration);

			countdown.gameObject.SetActive(false);
			closeBtn.gameObject.SetActive(false);
			closeBtn.onClick.RemoveListener(Close);

			winButton.gameObject.SetActive(true);
			winButton.onClick.AddListener(Win);
		}

		protected void Win()
		{
			Debug.Log("[" + nameof(Advert) + "] Win " + gameObject.name + " in " + TimeSinceOpen);
			OnWin?.Invoke(TimeSinceOpen);
			Close();
		}

		protected void Update()
		{
			int lInt = Mathf.CeilToInt(challengeDuration - TimeSinceOpen);
			lInt = Mathf.Max(lInt, 0);
			
			countdown.text = "Reward in : "+lInt.ToString();
		}

		protected override void UnRegisterEvents()
		{
			base.UnRegisterEvents();
			OnWin = null;
			winButton.onClick.RemoveListener(Win);

		}
	}
}