using UnityEngine;

public class SavePlayer : MonoBehaviour, SaveableObject
{
    public string InstanceIDDraft = "Player";
    public string InstanceID{
        get => InstanceIDDraft;
        set => InstanceIDDraft = value;
    }
    public int TypeID => int.MaxValue;
    public bool DoNotInstantiate => true;
    public string ObjectType => "PlayerStats";
    public bool SaveTransform => true;

    public string Save(){
        PlayerStats SavedStats = new PlayerStats();

        SavedStats.Health = GameServices.GlobalVariables.Player.PlayerHealth.Health;
        SavedStats.Hunger = 100;
        SavedStats.Thirst = 100;

        string json = JsonUtility.ToJson(SavedStats);

        return json;
    }

    // Update is called once per frame
    public void Load(string Json){
        PlayerStats LoadedStats = JsonUtility.FromJson<PlayerStats>(Json);

        GameServices.GlobalVariables.Player.PlayerHealth.Health = LoadedStats.Health;
    }

    [System.Serializable]
    public class PlayerStats{
        public float Health;
        public float Hunger;
        public float Thirst;
    }
}
