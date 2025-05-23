using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Tarabish.Definitions;
using Tarabish.Extensions;
using UnityEngine.EventSystems;
using Tarabish.Controllers;
using Tarabish.UI.Playing;

namespace Tarabish.Mechanics {

	[System.Serializable] public class CardIndexObject {
		public RectTransform Top;
		public RectTransform Bottom;
	}

	[System.Serializable] public class CardObjectReferences {
		public RectTransform Card;
		public TextMeshProUGUI ControllerName;
		public RectTransform Background;
		public RectTransform Backside;
		public RectTransform Content;
		public RectTransform Suit;
		public CardIndexObject Index;
	}

	public class Card : MonoBehaviour {
		public CardInfo Info;

		[Header("Display Settings")]
		public bool FaceUp = false;

		[Tooltip("The scale is multiplied by calculating the width and height individually like: (width*(1 + scale))")]
		[Range(-0.5f, 2.5f)] public float CardScale = 0;

		[Header("Runtime")]
		public CardHolderBase? CurrentHolder = null;
		public ControllerBase? Controller {
			get {
				if (!CurrentHolder) return null;
				return CurrentHolder.Controller;
			}
		}
		public int Score { get { return this.Info.Score; } }

		public CardObjectReferences CardReferences = new CardObjectReferences();

		//? Update all the info displayed on the card based on the current state of the variables 
		public void UpdateCard() {
			if (gameObject.scene.name == null) return; 

			if (!FaceUp) {
				CardReferences.Backside.gameObject.SetActive(true);
				return;
			}
			else {
				CardReferences.Backside.gameObject.SetActive(false);
			}

			Image suitImage = CardReferences.Suit.GetComponent<Image>();
			SuitInfo suitInfo = new();

			switch (Info.Suit) {
				case CardDefinitions.CardSuit.CLUB: 	{ suitInfo = CardDefinitions.ClubInfo; } break;
				case CardDefinitions.CardSuit.DIAMOND: 	{ suitInfo = CardDefinitions.DiamondInfo; } break;
				case CardDefinitions.CardSuit.HEART: 	{ suitInfo = CardDefinitions.HeartInfo; } break;
				case CardDefinitions.CardSuit.SPADE: 	{ suitInfo = CardDefinitions.SpadeInfo; } break;
				default: break;
			}

			//* Unity throws a false warning on this line that i cant disable...
			CardReferences.Card.sizeDelta = new Vector2(CardDefinitions.AspectRatio.x * (1 + CardScale), CardDefinitions.AspectRatio.y * (1 + CardScale));
			CardReferences.Suit.localScale = new Vector3(CardDefinitions.SuitSize, CardDefinitions.SuitSize, 0);
			suitImage.color = suitInfo.Color;
			suitImage.sprite = suitInfo.Image;

			SetCardIndex(suitInfo);

			// CardReferences.BoxCollider.size = CardReferences.Card.sizeDelta;
			// CardReferences.BoxCollider.offset = new Vector2(CardReferences.Card.sizeDelta.x / 2, -(CardReferences.Card.sizeDelta.y / 2));
		}

		//? This is a seperate function because it was getting pretty big and i want to seperate it from the rest
		private void SetCardIndex(SuitInfo suitInfo) {
			TextMeshProUGUI topText = CardReferences.Index.Top.GetChild(0).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI bottomText = CardReferences.Index.Bottom.GetChild(0).GetComponent<TextMeshProUGUI>();

			topText.text = CardDefinitions.GetIndexString(Info.Index);
			bottomText.text = CardDefinitions.GetIndexString(Info.Index);
			
			topText.color = suitInfo.Color;
			bottomText.color = suitInfo.Color;

			topText.transform.localScale = new Vector3(CardDefinitions.IndexSize, CardDefinitions.IndexSize, 0);
			bottomText.transform.localScale = new Vector3(CardDefinitions.IndexSize, CardDefinitions.IndexSize, 0);
		}


public void OnPointerClick() {
    this.LogVerbose($"{CardDefinitions.GetCardLogPrefix(Info)} OnPointerClick()");

    // Ensure only valid cards are processed
    if (GameManager.Instance.GameData.ActiveController.Controller == CurrentHolder.Controller) {
			CurrentHolder.CardInteraction = false;

        if (!GameManager.Instance.IsCardValidEntry(this)) {
            this.Log(new LogLineContent("Invalid Card Entry", ProjectManager.LogLevels.NOTIFY, this.Controller, this.Info));
			CurrentHolder.CardInteraction = true;
        } else {
            // Centralize handling in AI or player logic
            ProcessCardInteraction();
        }
    }
}

// Centralized interaction logic
private void ProcessCardInteraction() {
    if (!CardManager.Instance.HandleCardInteraction(this)) {
        Debug.LogWarning($"{CardDefinitions.GetCardLogPrefix(Info)} Interaction failed");
    }
}



		private void OnValidate() {
			UpdateCard();
		}

	}
}
