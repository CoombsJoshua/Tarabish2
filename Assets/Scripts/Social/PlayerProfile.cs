using UnityEngine;
[System.Serializable]
public class PlayerProfile
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Id { get; private set; }

    [field: SerializeField] public int Level { get; private set; } = 1;
    [field: SerializeField] public int XP { get; private set; } = 0;
    [field: SerializeField] public int Gold { get; private set; } = 0;

    [field: SerializeField] public int Wins { get; private set; } = 0;
    [field: SerializeField] public int Losses { get; private set; } = 0;

    // New Trophies field
    [field: SerializeField] public int Trophies { get; private set; } = 0;

    public PlayerProfile(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public void AddXP(int amount)
    {
        XP += amount;
        Debug.Log($"Added {amount} XP. Current XP: {XP}");

        // Level up if XP exceeds threshold
        int xpToNextLevel = Level * 100; // Example XP formula
        while (XP >= xpToNextLevel)
        {
            XP -= xpToNextLevel;
            Level++;
            Debug.Log($"Leveled up! New Level: {Level}");
            xpToNextLevel = Level * 100;
        }
    }

    public void EarnGold(int amount)
    {
        Gold += amount;
        Debug.Log($"Earned {amount} gold. Current Gold: {Gold}");
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            Debug.Log($"Spent {amount} gold. Remaining Gold: {Gold}");
            return true;
        }
        Debug.LogWarning("Not enough gold.");
        return false;
    }

    public void IncrementWins()
    {
        Wins++;
        Debug.Log($"Incremented wins. Total Wins: {Wins}");
    }

    public void IncrementLosses()
    {
        Losses++;
        Debug.Log($"Incremented losses. Total Losses: {Losses}");
    }

    public void AddTrophies(int amount)
    {
        Trophies += amount;
        Debug.Log($"Added {amount} trophies. Current Trophies: {Trophies}");
    }

	public void AddWins(int amount){
		Wins += amount;
	}

	public void AddLosses(int amount){
		Losses += amount;
	}

    public void RemoveTrophies(int amount)
    {
        Trophies = Mathf.Max(Trophies - amount, 0);
        Debug.Log($"Removed {amount} trophies (or as many as we could). Current Trophies: {Trophies}");
    }

    public override string ToString()
    {
        return $"{Name} (Id: {Id}, Level: {Level}, XP: {XP}, Gold: {Gold}, Wins: {Wins}, Losses: {Losses}, Trophies: {Trophies})";
    }
}
