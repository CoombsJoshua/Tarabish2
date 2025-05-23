using System.Collections.Generic;
using UnityEngine;


namespace Tarabish.Definitions {
	public class CreditDefinitions : MonoBehaviour {
		public enum CreditCategory {
			GameDesigner,
			Developer,
			Networking,
			GraphicDesign,
			Contributor,
			Special,
		}

		public class CreditInfo {
			public string Name;
			public string Nickname = null;
			public string Description;
			public string Reference;
			public CreditCategory Category;

			public string GetName() {
				return (this.Nickname != null) ? this.Nickname : this.Name;
			}
		}

		public static List<CreditInfo> Credits = new List<CreditInfo>() {
			new CreditInfo() {
				Name = "Atlas",
				Description = "Project Leader",
				Reference = "https://github.com/CoombsJoshua",
				Category = CreditCategory.Developer | CreditCategory.Networking
			},
			new CreditInfo() {
				Name = "CTNOriginals",
				Nickname = "CTN",
				Description = "Cleint-Side Developer",
				Reference = "https://github.com/CTNOriginals",
				Category = CreditCategory.GameDesigner | CreditCategory.Developer | CreditCategory.GraphicDesign
			},
			new CreditInfo() {
				Name = "Ashurst",
				Nickname = "Ash",
				Description = "Game Rule Analyst",
				Category = CreditCategory.Contributor
			},
			new CreditInfo() {
				Name = "kirkmac",
				Description = "Reference Game Developer",
				Reference = "https://kirkmac.itch.io/bish",
				Category = CreditCategory.Special
			},
			new CreditInfo() {
				Name = "treesgobark",
				Nickname = "treesbark",
				Description = "Asisted with complex debugging",
				Category = CreditCategory.Special
			},
		};

				public static List<CreditInfo> AIs = new List<CreditInfo>() {
			new CreditInfo() {
				Name = "Clyde",
			},
			new CreditInfo() {
				Name = "Claudette",

			},
			new CreditInfo() {
				Name = "Deborah",
			},
			new CreditInfo() {
				Name = "Fred",
			},
			new CreditInfo() {
				Name = "Heather",
			},
			new CreditInfo() {
				Name = "Jennifer",
			},
			new CreditInfo() {
				Name = "Amanda",
			},
			new CreditInfo() {
				Name = "Blaze",
			},
			new CreditInfo() {
				Name = "Sharon",
			},
		};

		private static CreditInfo _placeholderCredit = new CreditInfo() {
			Name = "Tera",
			Description = "PlaceHolder",
			Category = CreditCategory.Special,
		};
		public static CreditInfo GetCreditPlayer() {
			System.Random rng = new System.Random();
			List<CreditInfo> rngCredits = new List<CreditInfo>(Credits);
			for (int i = rngCredits.Count - 1; i > 0; i--) {
				int swapIndex = rng.Next(i + 1);
				CreditInfo _ = rngCredits[i];
				rngCredits[i] = rngCredits[swapIndex];
				rngCredits[swapIndex] = _;
			}

			foreach (CreditInfo credit in rngCredits) {
				if (GameManager.Instance.GameData.Players.Find((p) => p.name.Contains(credit.Name) || p.name.Contains(credit.Nickname)) == null) {
					return credit;
				}
			}

			return _placeholderCredit;
		}

				public static CreditInfo GetAIPlayer() {
			System.Random rng = new System.Random();
			List<CreditInfo> rngCredits = new List<CreditInfo>(AIs);
			for (int i = rngCredits.Count - 1; i > 0; i--) {
				int swapIndex = rng.Next(i + 1);
				CreditInfo _ = rngCredits[i];
				rngCredits[i] = rngCredits[swapIndex];
				rngCredits[swapIndex] = _;
			}

			foreach (CreditInfo credit in rngCredits) {
				if (GameManager.Instance.GameData.Players.Find((p) => p.name.Contains(credit.Name) || p.name.Contains(credit.Nickname)) == null) {
					return credit;
				}
			}

			return _placeholderCredit;
		}
	}
}