using System.Collections.Generic;
using System.Linq;
using Tarabish.Extensions;
using Tarabish.Mechanics;
using Tarabish.Utilities;
using UnityEngine;

namespace Tarabish.Definitions {
	public class SuitInfo {
		public Sprite Image;
		public Color Color;
	}

	[System.Serializable] public class SuitColorProfileClass {
		public Color CLUB; //> Default: #088608
		public Color DIAMOND; //> Default: #085886
		public Color HEART; //> Default: #F23731
		public Color SPADE; //> Default: #303030

		public SuitColorProfileClass(string c, string d, string h, string s) {
			ColorUtility.TryParseHtmlString(c, out CLUB);
			ColorUtility.TryParseHtmlString(d, out DIAMOND);
			ColorUtility.TryParseHtmlString(h, out HEART);
			ColorUtility.TryParseHtmlString(s, out SPADE);
		}
	}

	//? To be able to display the dictonary in the inspector
	[System.Serializable] public class IndexScoreField {
		public string Name;
		public int Score;

		public IndexScoreField(string name, int score) {
			Name = name;
			Score = score;
		}
	}
	[System.Serializable] public class IndexScoreVariantField {
		public string Name;
		public List<IndexScoreField> Scores = new List<IndexScoreField>();

		public IndexScoreVariantField(string name, List<IndexScoreField> scores = null) {
			Name = name;
			Scores = (scores != null) ? scores : new List<IndexScoreField>();
		}
	}

	public class CardDefinitions : Singleton<CardDefinitions> {
		#region Enums
		public enum CardIndex {
			TWO 	= 2,
			THREE 	= 3,
			FOUR 	= 4,
			FIVE 	= 5,
			SIX 	= 6,
			SEVEN 	= 7,
			EIGHT 	= 8,
			NINE 	= 9,
			TEN 	= 10,
			JACK 	= 11,
			QUEEN 	= 12,
			KING 	= 13,
			ACE 	= 14,
		}
		[System.Flags] public enum CardIndexFlags {
			TWO 	= 1 << 1,
			THREE 	= 1 << 2,
			FOUR 	= 1 << 3,
			FIVE 	= 1 << 4,
			SIX 	= 1 << 5,
			SEVEN 	= 1 << 6,
			EIGHT 	= 1 << 7,
			NINE 	= 1 << 8,
			TEN 	= 1 << 9,
			JACK 	= 1 << 10,
			QUEEN 	= 1 << 11,
			KING 	= 1 << 12,
			ACE 	= 1 << 13,
			ALL 	= TWO | THREE | FOUR | FIVE | SIX | SEVEN | EIGHT | NINE | TEN | JACK | QUEEN | KING | ACE
		}

		public enum CardSuit {
			CLUB,
			HEART,
			DIAMOND,
			SPADE,
		}
		[System.Flags] public enum CardSuitFlags {
			CLUB 	= 1 << 1,
			HEART 	= 1 << 2,
			DIAMOND = 1 << 3,
			SPADE 	= 1 << 4,
			ALL 	= CLUB | HEART | DIAMOND | SPADE
		}

		public enum SuitImageTypeEnum { TwoSided, Mono }
		public enum CardStatus {
			DECK,
			ACTIVE,
			POOL,
			USED,
		}
		#endregion

		#region Static Fields
		public static Vector2 AspectRatio = new Vector2(137, 187);
		public static float SuitSize = 1;
		public static float IndexSize = 1;

		public static SuitInfo ClubInfo = new();
		public static SuitInfo DiamondInfo = new();
		public static SuitInfo HeartInfo = new();
		public static SuitInfo SpadeInfo = new();

		public static List<IndexScoreVariantField> IndexScoreVariants = new List<IndexScoreVariantField>();
		#endregion

		#region Inspector
			#region Settings
			[Tooltip("TwoSided: The images are white as base and slightly grey for a shadow detail\nMono: The image is a solid white color and will only use a single color")]
			[SerializeField] private SuitImageTypeEnum _suitImageType = SuitImageTypeEnum.TwoSided;

			[Tooltip("Start after Assets/Resources/.\nIt should be the directory that includes the 3 image type directories")]
			[SerializeField] private string SuitImageRoot = $"Sprites/Suits/";

			[Header("Sizes")]
			//? This can be used for scaling up and down consistently
			//* As of now the ratio is 137:187, this is what google says to be the general ratio for a standard deck of cards
			[SerializeField] private Vector2 _aspectRatio = new Vector2(137, 187);
			[SerializeField, Range(0.5f, 1.5f)] private float _suitSize = 1;
			[SerializeField, Range(0.5f, 1.5f)] private float _indexSize = 1;

			[Header("Suit Colors")]
			public List<SuitColorProfileClass> SuitColorProfiles = new List<SuitColorProfileClass>() {
				new SuitColorProfileClass("#088608", "#085886", "#F23731", "#303030"),
				new SuitColorProfileClass("#282828", "#FF0000", "#FF0000", "#282828"),
				new SuitColorProfileClass("#085886", "#F23731", "#F23731", "#085886"),
			};
			[SerializeField] private int _activeColorProfileIndex = 0;
			public SuitColorProfileClass ActiveSuitColorProfile;

			[Header("Scores")]
			[SerializeField] private List<IndexScoreVariantField> _indexScoreVariants = new List<IndexScoreVariantField>();
			#endregion

			#region Debug
			[Header("Editor Settings")]
			[SerializeField] private bool _updateCardsOnValidate = false;

			[Header("Debug")]
			//? A string that holds a combination of settings to be able to compare and check if images need to be reloaded
			//* This will be a very short string for now, but there is a good chance there will be more to it later, otherwise i have once again overcomplecated this...
			[SerializeField] private string _suitImageVarientHash;

			[SerializeField] private Sprite _clubImage;
			[SerializeField] private Sprite _diamondImage;
			[SerializeField] private Sprite _heartImage;
			[SerializeField] private Sprite _spadeImage;
			#endregion
		#endregion
		
		#region Public Methods
		public static CardSuitFlags CardSuitToFlag(CardSuit suit) {
			return System.Enum.Parse<CardSuitFlags>(suit.ToString());
		}
		public static string GetIndexString(CardIndex index) {
			//? Add a space before the string to server as a left margin in the end result
			//TODO Figure out why the ace is the only character that doesnt have the same distance from the left as the other with a space
				//?? I think it has something to do with the font but its weird to me that a space is not consistant or that the A is way out of bounds
			string text;
			if ((int)index <= 10) {
				text = ((int)index).ToString();

				if ((int)index == 6 || (int)index == 9) {
					text = $"<u>{text}</u>";
				}
			}
			else {
				text = index.ToString()[0].ToString();
			}

			return " " + text;
		}
		public static SuitInfo GetSuitInfo(CardSuit suit) {
			return suit switch {
				CardSuit.HEART => HeartInfo,
				CardSuit.DIAMOND => DiamondInfo,
				CardSuit.SPADE => SpadeInfo,
				_ => ClubInfo,
			};
		}

		public static int GetCardValue(CardInfo info) {
			List<IndexScoreField> scores = IndexScoreVariants.Find((v) => v.Name == "DEFAULT").Scores;

			if (info.Suit == GameManager.Instance.GameData.TrumpSuit) {
				scores = IndexScoreVariants.Find((v) => v.Name == "TRUMP").Scores;
			}

			return scores.Find((s) => s.Name == info.Index.ToString()).Score;
		}

		public static Card CompareCards(Card a, Card b) {
			/* TieBreaker scenario (Trump:Club | Picked by Entry 4)
				- cards in pool: [S:A] [H:J] [C:8] [C:7] (Tie: [C:8] [C:7] = Value(0|0))
				> 	[C:8] wins because it was first entered
				> 	[C:7] wins because they picked the trump suit
				> 	[C:7] lost because they picked the trump suit
				> 	[S:A] winst because it is the next best card (unlikely pick)
				() 	Incase of 4 players
				> 		[S:A][C:8] wins because their total score is 11 against 2
			*/

			CardSuit trumpSuit = GameManager.Instance.GameData.TrumpSuit;
			return (Card)GeneralUtilities.CompareScores(a, b, a.Score, b.Score, (a.Info.Suit == trumpSuit), (b.Info.Suit == trumpSuit));
		}
		public static bool IsBetterCard(Card a, Card b) {
			return CompareCards(a, b) == a;
		}

		public static Sprite GetSuitImageByCardSuit(CardSuit suit) {
			return suit switch {
				CardSuit.DIAMOND => DiamondInfo.Image,
				CardSuit.HEART => HeartInfo.Image,
				CardSuit.SPADE => SpadeInfo.Image,
				_ => ClubInfo.Image,
			};
		}
		public static Color GetColorByCardSuit(CardSuit suit) {
			return suit switch {
				CardSuit.DIAMOND => DiamondInfo.Color,
				CardSuit.HEART => HeartInfo.Color,
				CardSuit.SPADE => SpadeInfo.Color,
				_ => ClubInfo.Color,
			};
		}

		public static string GetCardLogPrefix(CardInfo info, string suffix = "\n") {
			string colorHex = (info.Suit == CardSuit.SPADE) ? "FFFFFF" : ColorUtility.ToHtmlStringRGB(GetSuitInfo(info.Suit).Color);
			
			return GeneralUtilities.FormatLogPrefix(new object[] {
				$"<color=#{colorHex}>{((GameManager.Instance.GameData.TrumpSuit == info.Suit) ? $"<b><u>{info.Suit.ToString()[0]}</u></b>" : $"{info.Suit.ToString()[0]}")}</color>",
				$"<color=#ce9178>{GetIndexString(info.Index).Trim()}</color>",
				$"<color=#00BB00>{info.Score}</color>"
			},suffix);
		}
		#endregion

		#region Private Methods
		protected override void Awake() {
			base.Awake();
			OnValidate();
		}

		private void LoadSuitImages() {
			//? Ensure the string ends with the "/" to avoid hard to understand errors
			if (SuitImageRoot[^1] != '/') { SuitImageRoot += "/"; }

			_clubImage = Resources.Load<Sprite>($"{SuitImageRoot}{_suitImageType}/CLUB");
			_diamondImage = Resources.Load<Sprite>($"{SuitImageRoot}{_suitImageType}/DIAMOND");
			_heartImage = Resources.Load<Sprite>($"{SuitImageRoot}{_suitImageType}/HEART");
			_spadeImage = Resources.Load<Sprite>($"{SuitImageRoot}{_suitImageType}/SPADE");

			this.LogDebug($"Loaded suit images with path combination: {SuitImageRoot}{_suitImageType}/<SUIT_NAME>");
		}

		private void OnValidate() {
			string suitImageVarient = $"{SuitImageRoot}{_suitImageType}"; //? Add more details here when they alter the kind of image being loaded
			if (suitImageVarient != _suitImageVarientHash) {
				LoadSuitImages();
				this.LogDebug($"{suitImageVarient}\n\t{_suitImageVarientHash}");
				_suitImageVarientHash = suitImageVarient;
			}

			_activeColorProfileIndex = Mathf.Clamp(_activeColorProfileIndex, 0, SuitColorProfiles.Count - 1);
			ActiveSuitColorProfile = SuitColorProfiles[_activeColorProfileIndex];

			AspectRatio = _aspectRatio;
			SuitSize = _suitSize;
			IndexSize = _indexSize;

			ClubInfo.Color = ActiveSuitColorProfile.CLUB;
			DiamondInfo.Color = ActiveSuitColorProfile.DIAMOND;
			HeartInfo.Color = ActiveSuitColorProfile.HEART;
			SpadeInfo.Color = ActiveSuitColorProfile.SPADE;

			ClubInfo.Image = _clubImage;
			DiamondInfo.Image = _diamondImage;
			HeartInfo.Image = _heartImage;
			SpadeInfo.Image = _spadeImage;

			// HandleIndexScores();
			if (_indexScoreVariants.Count == 0) {
				_indexScoreVariants = new List<IndexScoreVariantField>() {
					new IndexScoreVariantField("DEFAULT", new List<IndexScoreField>() {
						new IndexScoreField("SIX", 0),
						new IndexScoreField("SEVEN", 0),
						new IndexScoreField("EIGHT", 0),
						new IndexScoreField("NINE", 0),
						new IndexScoreField("TEN", 10),
						new IndexScoreField("JACK", 2),
						new IndexScoreField("QUEEN", 3),
						new IndexScoreField("KING", 4),
						new IndexScoreField("ACE", 11),
					}),
					new IndexScoreVariantField("TRUMP", new List<IndexScoreField>() {
						new IndexScoreField("SIX", 0),
						new IndexScoreField("SEVEN", 0),
						new IndexScoreField("EIGHT", 0),
						new IndexScoreField("NINE", 14),
						new IndexScoreField("TEN", 10),
						new IndexScoreField("JACK", 20),
						new IndexScoreField("QUEEN", 3),
						new IndexScoreField("KING", 4),
						new IndexScoreField("ACE", 11),
					})
				};
			}

			IndexScoreVariants = _indexScoreVariants;

			if (_updateCardsOnValidate) CardManager.Instance?.UpdateCardsInScene();
		}
		#endregion
	}
}