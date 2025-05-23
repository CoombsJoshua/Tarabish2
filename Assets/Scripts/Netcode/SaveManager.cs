using UnityEngine;

namespace Tarabish.Netcode {
	public static class SaveManager
	{
		private const string SessionKey = "SD";
		
		public static void SaveSession(SessionData sessionData){
			string jsonData = JsonUtility.ToJson(sessionData);
			PlayerPrefs.SetString(SessionKey, jsonData);
			PlayerPrefs.Save();
		}
	
		public static SessionData LoadSession(){
			if(PlayerPrefs.HasKey(SessionKey)){
				string jsonData = PlayerPrefs.GetString(SessionKey);
				return JsonUtility.FromJson<SessionData>(jsonData);
			} else{
				return new SessionData("Guest", 0,1,0,0);
			}
		}
	}
}
