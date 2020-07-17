using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Com.Github.Knose1.Common.FakeAdvert.UI.AdUi {
	public class Cookie : MonoBehaviour, IEventSystemHandler, IPointerDownHandler
	{
		[SerializeField] protected ParticleSystem cookieParticlePrefab;
		[SerializeField] protected Transform particleContainer;

		private void OnDisable()
		{
			for (int i = particleContainer.childCount - 1; i >= 0; i--)
			{
				Destroy(particleContainer.GetChild(i).gameObject);
			}
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			Instantiate(cookieParticlePrefab, particleContainer);
		}
	}
}