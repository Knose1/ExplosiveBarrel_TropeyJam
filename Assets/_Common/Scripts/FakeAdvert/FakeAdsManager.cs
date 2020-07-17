using Com.Github.Knose1.Common.Common;
using Com.Github.Knose1.Common.FakeAdvert.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.FakeAdvert {
	public class FakeAdsManager : MonoBehaviour {

		public static FakeAdsManager Instance => Singleton.GetInstance<FakeAdsManager>();

		[SerializeField] protected List<Advert> adverts;
		
		protected List<RewardAdvert> rewardAdverts = new List<RewardAdvert>();
		protected List<Advert> fullScreenAdverts = new List<Advert>();
		protected List<Advert> sideBarAdverts = new List<Advert>();

		protected void Awake()
		{
			for (int i = adverts.Count - 1; i >= 0; i--)
			{
				Advert advert = adverts[i];
				advert.Close();

				if (advert is RewardAdvert) rewardAdverts.Add(advert as RewardAdvert);
				else if (advert.Placement == Advert.PlacementType.Fullscreen) fullScreenAdverts.Add(advert);
				else if (advert.Placement == Advert.PlacementType.SideBar) sideBarAdverts.Add(advert);
			}
		}

		private void Start()
		{
			if (FakeAdsContainer.Instance == null)
			{
				Debug.LogWarning("There is no instance of "+nameof(FakeAdsContainer)+" on the scene");
			}
		}

		protected bool LogErrorIfNoInstanceOfFakeAdsContainer()
		{
			if (!FakeAdsContainer.Instance)
			{
				Debug.LogError("There is no instance of " + nameof(FakeAdsContainer) + " on the scene");
			}

			return !FakeAdsContainer.Instance;
		}

		public Advert DisplayFullScreenAdvert()
		{
			if (LogErrorIfNoInstanceOfFakeAdsContainer()) return null;

			if (fullScreenAdverts.Count < 1)
			{
				Debug.Log("[" + nameof(FakeAdsManager) + "] There is not Full screen advert to display");
				return null;
			}

			Advert ad = fullScreenAdverts[Random.Range(0, fullScreenAdverts.Count)];

			FakeAdsContainer.Instance.ShowAdvert(ad);

			return ad;
		}

		public Advert DisplaySideBarAdvert()
		{
			if (LogErrorIfNoInstanceOfFakeAdsContainer()) return null;

			if (sideBarAdverts.Count < 1)
			{
				Debug.Log("[" + nameof(FakeAdsManager) + "] There is not side bar advert to display");
				return null;
			}

			Advert ad = sideBarAdverts[Random.Range(0, sideBarAdverts.Count)];

			FakeAdsContainer.Instance.ShowAdvert(ad);

			return ad;
		}

		public RewardAdvert DisplayRewardAdvert(RewardAdvert.WinDelegate onWin)
		{
			if (LogErrorIfNoInstanceOfFakeAdsContainer()) return null;

			if (rewardAdverts.Count < 1)
			{
				Debug.Log("["+nameof(FakeAdsManager) +"] There is not Reward advert to display");
				return null;
			}

			RewardAdvert ad = rewardAdverts[Random.Range(0, rewardAdverts.Count)];
			ad.OnWin += onWin;

			FakeAdsContainer.Instance.ShowAdvert(ad);

			return ad;

		}
	}
}