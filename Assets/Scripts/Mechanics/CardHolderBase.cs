using System;
using System.Collections.Generic;
using Tarabish.Controllers;
using Tarabish.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Tarabish.Mechanics {
	public class CardHolderBase : MonoBehaviour {
		public enum CardHolderType { HAND, POOL_SLOT }
		public CardHolderType Type {
			get {
				if (this.HasComponent<Hand>()) { return CardHolderType.HAND; }
				if (this.HasComponent<PoolSlot>()) { return CardHolderType.POOL_SLOT; }

				return default;
			}
		}

		public ControllerBase Controller;

	    public List<Card> Cards = new List<Card>();

		private bool _cardInteraction = false;
		public bool CardInteraction {
			get {
				return _cardInteraction;
			}
			set {
				foreach (Card card in Cards) {
					card.GetComponent<EventTrigger>().enabled = value;
				}
				_cardInteraction = value;
			}
		}

		public virtual void AddCard(Card card, bool faceUp = true) {
			card.transform.SetParent(this.transform);
			card.transform.SetPositionAndRotation(this.transform.position, new Quaternion(0,0,0,0));
			card.FaceUp = faceUp;
			card.CardReferences.ControllerName.text = Controller.Info.Name;

			Cards.Add(card);
			card.UpdateCard();
		}
		public virtual void AddCard(CardInfo info, bool faceUp) {
			AddCard(CardManager.Instance.GenerateCard(info), faceUp);
		}

		public virtual void RemoveCard(Card card) {
			Cards.Remove(card);
			DeckManager.Instance.ActiveCards.ReturnCard(card);
		}
		public virtual void RemoveCard(CardInfo info) {
			RemoveCard(Cards.Find((c) => c.Info == info));
		}

		public virtual void TransferCard(Card card, CardHolderBase cardHolder, bool faceUp = true) {
			cardHolder.AddCard(card, faceUp);
			Cards.Remove(card);
		}
		public virtual void TransferCard(CardInfo info, CardHolderBase cardHolder) {
			TransferCard(Cards.Find((c) => c.Info == info), cardHolder);
		}

		public virtual void Clear() {
			for (int i = 0; i < Cards.Count; i++) {
				RemoveCard(Cards[i]);
			}
		}

		/// <summary>
		/// Toggle all cards to be Face(Up:true/Down:false)
		/// </summary>
		/// <param name="value">true for FaceUp, false for FaceDown</param>
		public virtual void SetFaceUp(bool value) {
			foreach (Card card in Cards) {
				card.FaceUp = value;
				card.UpdateCard();
			}
		}
	}
}
