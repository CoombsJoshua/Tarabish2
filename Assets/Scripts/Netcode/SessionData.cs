namespace Tarabish.Netcode {
	[System.Serializable]
	public class SessionData{
		public string Username;
		public int ProfilePictureID;
	
		public int GamesWon;
		public int GamesLost;
	
		// Construct
		public SessionData(string username, int profilePictureID, int level, int GamesWon, int GamesLost){
			Username = username;
			ProfilePictureID = profilePictureID;
			this.GamesWon = GamesWon;
			this.GamesLost = GamesLost;
		}
	
		public bool IsNameSet{
			get {return !Username.Equals("Guest", System.StringComparison.OrdinalIgnoreCase);}
		}
	}
}