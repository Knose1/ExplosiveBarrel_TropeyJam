///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 07/06/2020 15:30
///-----------------------------------------------------------------

using Com.Github.Knose1.ExploseEm.Game;
using Com.Github.Knose1.ExploseEm.Game.View;
using DG.Tweening;
using DG;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Github.Knose1.ExploseEm.UI {
	public class Hud : MonoBehaviour {

		[SerializeField] Text bombText = null;
		[SerializeField] Text rockText = null;
		[SerializeField] Text siliciumText = null;
		[SerializeField] Text steelText = null;
		[SerializeField] Text scoreText = null;

		[SerializeField] float twinTime = 1;

		bool isTwinFinished = true;

		int oldLineDropValue;
		int oldBombValue;
		int oldRockValue;
		int oldSiliciumValue;
		int oldSteelValue;

		private void Awake()
		{
			GameManager.OnStart += GameManager_OnStart;
			//GameManager.OnExplosion += GameManager_OnExplosion;
			GameManager.OnExplosionEnd += GameManager_OnExplosionEnd;
			GameManager.OnCraftEnd += GameManager_OnCraftEnd;
			GridView.OnSetBombs += GridView_OnSetBombs;
		}
		private void GameManager_OnExplosion(GameManager gameManager, System.Collections.Generic.List<Vector2> arg2, Vector2 arg3)
		{
			DOTween.To(() => { return oldRockValue; }, (int value) => { SetRock(oldRockValue = value); }, gameManager.RockCount, 1);
			DOTween.To(() => { return oldSiliciumValue; }, (int value) => { SetSilicium(oldSiliciumValue = value); }, gameManager.SiliciumCount, 1);
			DOTween.To(() => { return oldSteelValue; }, (int value) => { SetSteel(oldSteelValue = value); }, gameManager.SteelCount, 1);
		}

		private void GameManager_OnStart(GameManager gameManager)
		{

			SetLineDrop(oldLineDropValue = gameManager.LineDrop);
			SetRock(oldRockValue = gameManager.RockCount);
			SetSilicium(oldSiliciumValue = gameManager.SteelCount);
			SetSteel(oldSteelValue = gameManager.SiliciumCount);
			SetBomb(oldBombValue = gameManager.BombCount);
		}
		private void GameManager_OnExplosionEnd(GameManager gameManager)
		{
			TweenMonney(gameManager);
			DOTween.To(() => { return oldLineDropValue; }, (int value) => { SetLineDrop(oldLineDropValue = value); }, gameManager.LineDrop, twinTime);
		}

		private void GameManager_OnCraftEnd(GameManager gameManager)
		{
			StartCoroutine(Coroutine());

			IEnumerator Coroutine()
			{
				yield return new WaitUntil(() => { return isTwinFinished; });
				TweenMonney(gameManager);

				DOTween.To(() => { return oldBombValue; }, (int value) => { SetBomb(oldBombValue = value); }, gameManager.BombCount, twinTime);

			};
		}

		private void TweenMonney(GameManager gameManager)
		{
			isTwinFinished = false;


			DOTween.To(() => { return oldRockValue; }, (int value) => { SetRock(oldRockValue = value); }, gameManager.RockCount, twinTime);
			DOTween.To(() => { return oldSiliciumValue; }, (int value) => { SetSilicium(oldSiliciumValue = value); }, gameManager.SiliciumCount, twinTime);
			DG.Tweening.Core.TweenerCore<int, int, DG.Tweening.Plugins.Options.NoOptions> twin = DOTween.To(() => { return oldSteelValue; }, (int value) => { SetSteel(oldSteelValue = value); }, gameManager.SteelCount, twinTime);
			
			twin.OnComplete(() => { isTwinFinished = true; });
		}

		private void GridView_OnSetBombs(int count)
		{
			SetBomb(oldBombValue = GameManager.Instance.BombCount - count);
		}

		private void SetBomb(int value) => bombText.text = value.ToString();
		private void SetLineDrop(int value) => scoreText.text = value.ToString();
		private void SetSteel(int value) => steelText.text = value + "/" + GameManager.Instance.RequiredSteel;
		private void SetSilicium(int value) => siliciumText.text = value + "/" + GameManager.Instance.RequiredSilicium;
		private void SetRock(int value) => rockText.text = value + "/" + GameManager.Instance.RequiredRock;

		private void OnDestroy()
		{
			GameManager.OnStart -= GameManager_OnStart;
			GameManager.OnExplosion -= GameManager_OnExplosion;
			GameManager.OnExplosionEnd -= GameManager_OnExplosionEnd;
			GridView.OnSetBombs -= GridView_OnSetBombs;
		}
	}
}