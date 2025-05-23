using System.Collections.Generic;
using Tarabish.Definitions;
using Tarabish.Extensions;
using Tarabish.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Tarabish.UI.Menu {
	public class Menu_Credits : Menu_ {
		public override MenuType MenuType => MenuType.Credits;

		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private GameObject _creditItemPrefab;
		[SerializeField] private List<CreditItem> _creditItems;

		private void OnEnable() {
			foreach (CreditDefinitions.CreditInfo credit in CreditDefinitions.Credits) {
				if (!_creditItems.Find((c) => c.Name.text == credit.Name)) {
					CreditItem item = Instantiate(_creditItemPrefab, _scrollRect.content).GetComponent<CreditItem>();
					item.Name.text = credit.Name;
					item.Description.text = credit.Description;

					_creditItems.Add(item);
				}
			}
		}
	}
}
