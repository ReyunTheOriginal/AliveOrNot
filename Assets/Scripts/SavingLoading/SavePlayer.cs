using UnityEngine;

public class SavePlayer : BasicSave
{
    public override string InstanceID => "Player";
    public override int TypeID => int.MaxValue;
    public override bool DoNotInstantiate => true;
    public override string ObjectType => "PlayerStats";
    public override bool SaveTransform => true;
    public override bool SavedByParent => false;
    public override bool CanBeSave => true;

    public override string Save(){
        PlayerStats SavedStats = new PlayerStats();

        SavedStats.Health = GameServices.GlobalVariables.Player.PlayerHealth.Health;
        SavedStats.Hunger = 100;
        SavedStats.Thirst = 100;

        string json = JsonUtility.ToJson(SavedStats);

        return json;
    }

    // Update is called once per frame
    public override void Load(string Json){
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
