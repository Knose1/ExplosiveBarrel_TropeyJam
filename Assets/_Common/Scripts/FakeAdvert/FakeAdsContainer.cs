using UnityEngine;
using Com.Github.Knose1.Common.FakeAdvert.UI;
using Com.Github.Knose1.Common.Common;

namespace Com.Github.Knose1.Common.FakeAdvert {
	public class FakeAdsContainer : MonoBehaviour {

		public static FakeAdsContainer Instance => Singleton.GetInstance<FakeAdsContainer>();

		public const string DEBUG_TAG = "[FakeAds]";

		protected Advert currentFullscreenAdvert;
		protected Advert currentSideBarAdvert;

		public void ShowAdvert(Advert advert)
		{
			switch (advert.Placement)
			{
				case Advert.PlacementType.Fullscreen:
					SetFullscreenAdvert(advert);
					break;
				case Advert.PlacementType.SideBar:
					SetSideBarAdvert(advert);
					break;

				default:
					Debug.LogWarning(DEBUG_TAG+" The "+nameof(Advert.PlacementType)+" "+advert.Placement.ToString()+" is not handeled by the "+nameof(FakeAdsContainer));
					return;
			}
		}
		
		protected void SetFullscreenAdvert(Advert advert)
		{
			if (currentFullscreenAdvert)
			{
				Debug.LogWarning(DEBUG_TAG+" The last fullscreen advert wasn't closed");
				currentFullscreenAdvert.Close();
			}

			advert.Open();
			advert.OnClose += FullScreen_Advert_OnClose;

			currentFullscreenAdvert = advert;
		}

		protected void SetSideBarAdvert(Advert advert)
		{
			if (currentSideBarAdvert)
			{
				Debug.LogWarning(DEBUG_TAG + " The last side bar advert wasn't closed");
				currentSideBarAdvert.Close();
			}

			advert.Open();
			advert.OnClose += SideBar_Advert_OnClose;

			currentSideBarAdvert = advert;
		}

		private void FullScreen_Advert_OnClose(float openTime) => currentFullscreenAdvert = null;

		private void SideBar_Advert_OnClose(float openTime) => currentSideBarAdvert = null;
	}
}