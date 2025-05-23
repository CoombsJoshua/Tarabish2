using System.Collections.Generic;
using UnityEngine;

namespace Tarabish.Utilities {
	public class GeneralUtilities : MonoBehaviour {
		public static string FormatLogPrefix(object[] prefixes, string suffix = "\n") {
			string returnValue = "<color=#DDDDDD>[</color>";
			for (int i = 0; i < prefixes.Length; i++) {
				string str = prefixes[i].ToString();
				str = str.Trim();
				returnValue += str + $"{((i != prefixes.Length - 1) ? " <color=#FFFFFF>|</color> " : "")}";
			}
			return returnValue + $"<color=#DDDDDD>]</color>{suffix}";
		}
		public static string FormatLogPrefix(string prefix, string suffix = "\n") {
			return FormatLogPrefix(new string[] {prefix}, suffix);
		}

		public static object CompareScores(object a, object b, int AScore, int BScore, bool ATrump, bool BTrump) {
			if (
				(!BTrump && AScore > BScore) ||
				(ATrump && !BTrump) ||
				(ATrump && BTrump && AScore > BScore)
			) { return a; }
			else { return b; }
		}

		//?? idk how to make this work...
		// public static List<string> EnumFlagsAsArray<T>(T flags) {
		// 	List<string> enabledFlags = new List<string>();

		// 	foreach (string flag in System.Enum.GetNames(typeof(T))) {
		// 		if ((T is typeof(System.Enum))flags.HasFlag(System.Enum.Parse<T>(flag))) {
		// 			enabledFlags.Add(flag);
		// 		}
		// 	}

		// 	return enabledFlags;
		// } 
	}
}
