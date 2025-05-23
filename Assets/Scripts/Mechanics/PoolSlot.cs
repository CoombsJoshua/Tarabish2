using TMPro;
using UnityEngine;

namespace Tarabish.Mechanics {
	public class PoolSlot : CardHolderBase {
		public TextMeshProUGUI ControllerNameDisplay; // Reference to the UI text element for the controller's name.

		public Pool Pool = default;
		public Card? Card {
			get {
				return (Cards.Count > 0) ? Cards[0] : null;
			}
		}

		public override void AddCard(Card card, bool faceUp = true) {
			base.AddCard(card, faceUp);
			Pool.Cards.Add(card);
			card.CardReferences.ControllerName.transform.parent.gameObject.SetActive(true);
			DeckManager.Instance.ActiveCards.TransferCard(card.Info, Definitions.CardDefinitions.CardStatus.POOL);
		}

		public void UpdateControllerName() {
			if (Controller != null && Controller.Info != null) {
				ControllerNameDisplay.text = Controller.Info.Name;
				ControllerNameDisplay.gameObject.SetActive(true); // Ensure it's visible.
			} else {
				ControllerNameDisplay.text = "";
				ControllerNameDisplay.gameObject.SetActive(false); // Hide if no controller.
			}
		}

		public void Highlight(bool isDealer) {
    if (isDealer) {
        // Change the background or border color to indicate the dealer
        ControllerNameDisplay.color = Color.yellow; // Example: Yellow for the dealer
    } else {
        // Reset to default color
        ControllerNameDisplay.color = Color.white; // Default color
    }
}

	}
}
