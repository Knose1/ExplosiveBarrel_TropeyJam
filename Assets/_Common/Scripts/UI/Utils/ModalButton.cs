///-----------------------------------------------------------------
/// Author : Knose1
/// Date : 13/05/2020 12:50
///-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Common.UI.Utils {
	public class ModalButton : MonoBehaviour {

		public Text text = null;
		public Button button = null;

		public Button.ButtonClickedEvent OnClick
		{
			get
			{
				return button.onClick;
			}
			set
			{
				button.onClick = value;
			}
		}

		public void SetText(string text)
		{
			this.text.text = text;
		}


	}
}