///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 15/02/2021 23:27
///-----------------------------------------------------------------

using Com.Github.Knose1.Common.Common;
using Com.Github.Knose1.ExploseEm.Game;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace Com.Github.Knose1.ExploseEm.UI {
	public class GameOver : MonoBehaviour {

		private Animator animator = null;
		[SerializeField] private string scorePrefix = "Score:";
		[SerializeField] private string animatorBool = "Visible";
		[SerializeField] private float textTwinDuration = 1;
		[SerializeField] private Text score = null;
		[SerializeField] private Button btnRetry = null;

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void Start () {
			Singleton.GetInstance<GameManager>().OnGameOver += GameManager_OnGameOver;
			btnRetry.onClick.AddListener(BtnRetry_Click);
		}

		private void GameManager_OnGameOver(GameManager obj)
		{
			animator.SetBool(animatorBool, true);
			obj.OnGameOver -= GameManager_OnGameOver;
		}

		private void BtnRetry_Click()
		{
			animator.SetBool(animatorBool, false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void AnimateScore()
		{
			int currentScore = 0;
			int targetScore = Singleton.GetInstance<GameManager>().LineDrop;
			DOTween.To(() => currentScore, lSet, targetScore, textTwinDuration);


			void lSet(int pNewValue)
			{
				currentScore = pNewValue;
				score.text = scorePrefix + " " + currentScore;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void LoadRetryScene()
		{
			SceneManager.LoadScene(0, LoadSceneMode.Single);
		}

		private void OnDestroy()
		{
			btnRetry.onClick.RemoveListener(BtnRetry_Click);
		}
	}
}