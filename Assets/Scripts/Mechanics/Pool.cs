using System;
using System.Collections.Generic;
using Tarabish.Extensions;
using UnityEngine;

namespace Tarabish.Mechanics {
	public class Pool : MonoBehaviour {
		public List<PoolSlot> Slots = new List<PoolSlot>();
		public List<Card> Cards = new List<Card>(); //? We need this to be able to tell in which order the cards were entered

		private void Awake() {
			GetChildSlots(this.transform);
		}

		private void GetChildSlots(Transform parent) {
			foreach (Transform child in parent) {
				if (child.TryGetComponent(out PoolSlot slot)) {
					AddSlot(slot);
				}

				if (child.childCount > 0 && !child.HasComponent<Pool>()) {
					GetChildSlots(child);
				}
			}
		}
		private void AddSlot(PoolSlot slot) {
			Slots.Add(slot);
			slot.Pool = this;
		}

		public void Clear() {
			Cards.Clear();
			foreach (PoolSlot slot in Slots) {
				slot.Clear();
			}
		}
	}
}
