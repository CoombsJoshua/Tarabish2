using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Tarabish.Controllers;
using Tarabish.Extensions;
using Tarabish.Mechanics;
using Tarabish.UI;
using Tarabish.UI.Playing;

namespace Tarabish {
	public class ReferenseManager : Singleton<ReferenseManager> {
		[System.Serializable] public class ControllerObjectReferences {
			public Hand Hand;
			public PoolSlot PoolSlot;
    public Transform PopupAnchor; // New anchor for popup bubbles.

			public void SetController(ControllerBase controller) {
				Hand.Controller = controller;
				PoolSlot.Controller = controller;
			}
		}

		public RectTransform PlayingArea;
		public RectTransform PreviewArea;
		public RectTransform IdleCardHolder; //? When the cards are generated they first go here before they are assigned a parent
		public Transform Controllers;

		[Header("Menus")]
		public UI.Menu.Menu_Title MenuTitle;
		public UI.Menu.Menu_Settings MenuSettings;

		[Header("UI")]
		public GameLog GameLog;
		public GameScorePanel GameScorePanel;
		public RectTransform TrumpPickerObject;
		public Image TrumpSuitDisplay;
		public RectTransform Buttons;

		[Header("In Game")]
		public PlayerController LocalPlayer;
		public Pool Pool;

		[Header("Prefabs")]
		public GameObject CardPrefab;
		public GameObject CardHolderPrefab;
		public GameObject GameLogChunk;
		public GameObject GameLogLine;

		[Header("Controller")]
		public ControllerObjectReferences PlayerReferences;
		public List<ControllerObjectReferences> OtherReferences;
	}
}

