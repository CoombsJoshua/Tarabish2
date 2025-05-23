using System.Collections;
using Tarabish.Definitions;
using Tarabish.Extensions;
using Tarabish;
using Tarabish.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.UI.Playing {
	public class GameLogLine : MonoBehaviour {
		[System.Serializable] public class ObjectReferences {
			[System.Serializable] public class CardInfoReference {
				public RectTransform Holder;
				public Image Suit;
				public TextMeshProUGUI Index;
			}

			public Image SideColorBar;
			public RectTransform InfoHolder;
			public RectTransform PlayerName;
			public RectTransform TextHolder;
			public CardInfoReference CardInfo;

			public TextMeshProUGUI PlayerNameText => PlayerName.GetChild(0).GetComponent<TextMeshProUGUI>();
			public TextMeshProUGUI TextElement => TextHolder.GetComponent<TextMeshProUGUI>();

			public void SetInfoHolderState(bool state) {
				this.CardInfo.Holder.gameObject.SetActive(state);
				this.PlayerName.gameObject.SetActive(state);
			}
		}

		public ObjectReferences References;
		private RectTransform _rect => this.GetComponent<RectTransform>();
		
		[Header("Debug")]
		[SerializeField] private bool _test;
		[SerializeField, TextArea] private string _testText;

		public string Text {
			get { return this.References.TextElement.text; }
			set { this.References.TextElement.text = value; }
		}

		public void SetInfo(LogLineContent content) {
			this.Text = content.Text;
			this.References.SideColorBar.color = ReferenseManager.Instance.GameLog.LogLevelColors.Find((llc) => llc.Name == content.Level.ToString()).Color;

			if (content.CardInfo != null) {
				this.References.CardInfo.Holder.gameObject.SetActive(true);
				this.References.CardInfo.Suit.sprite = content.SuitInfo.Image;
				this.References.CardInfo.Suit.color = content.SuitInfo.Color;
				this.References.CardInfo.Index.text = CardDefinitions.GetIndexString(content.CardInfo.Index).TrimStart();

				if (content.CardInfo.Suit == GameManager.Instance.GameData.TrumpSuit) {
					this.References.CardInfo.Index.fontStyle = FontStyles.Bold;
				} else {
					this.References.CardInfo.Index.fontStyle = FontStyles.Normal;
				}

			} else { this.References.CardInfo.Holder.gameObject.SetActive(false); }

			if (content.Controller != null) {
				this.References.PlayerName.gameObject.SetActive(true);
				this.References.PlayerNameText.text = content.Controller.Info.Name;
			} else { this.References.CardInfo.Holder.gameObject.SetActive(false); }

			this.UpdateElements();
		}

		private void UpdateElements() {
			Canvas.ForceUpdateCanvases();

			this._rect.sizeDelta = new Vector2(_rect.sizeDelta.x, References.InfoHolder.sizeDelta.y + References.TextHolder.sizeDelta.y);
			this.References.TextHolder.offsetMax = new Vector2(
				References.TextHolder.offsetMax.x,
				_rect.sizeDelta.y - References.InfoHolder.sizeDelta.y
			);
		}

		private void OnValidate() {
			if (_test && Application.isPlaying) {
				this.Text = _testText;
				_test = false;
			}
		}
	}
}