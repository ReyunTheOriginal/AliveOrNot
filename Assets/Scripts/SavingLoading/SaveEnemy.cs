using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveEnemy : BasicSave
{
    public EnemyProperties Properties;
    public override string ObjectType => "Enemies";

    private void Awake() {
      if (!Properties)Properties = GetComponent<EnemyProperties>();  
    }

    public override string Save(){
        SavedEnemyInfo EnemyInfo = new SavedEnemyInfo();

        EnemyInfo.Health = Properties.GetHealth();

        EnemyInfo.Chasing = Properties.CurrentStates[EnemyProperties.State.Chasing];
        EnemyInfo.Idle = Properties.CurrentStates[EnemyProperties.State.Idle];
        EnemyInfo.Roaming = Properties.CurrentStates[EnemyProperties.State.Roaming];

        EnemyInfo.CurrentDirection = (int)Properties.CurrentDirection;

        foreach(var effect in Properties.CurrentEffects){
            EnemyInfo.CurrentEffects.Add((int)effect.Key);
            EnemyInfo.CurrentEffectsDuration.Add(effect.Value);
        }


        return JsonUtility.ToJson(EnemyInfo);
    }

    public override void Load(string Json){
        SavedEnemyInfo EnemyInfo = JsonUtility.FromJson<SavedEnemyInfo>(Json);

        Properties.SetHealth(EnemyInfo.Health);

        Properties.CurrentStates[EnemyProperties.State.Roaming] = EnemyInfo.Roaming;
        Properties.CurrentStates[EnemyProperties.State.Chasing] = EnemyInfo.Chasing;
        Properties.CurrentStates[EnemyProperties.State.Idle] = EnemyInfo.Idle;

        Properties.CurrentDirection = (EnemyProperties.Direction)EnemyInfo.CurrentDirection;

        for(int i=0;i<EnemyInfo.CurrentEffects.Count;i++){
            Properties.CurrentEffects.Add((EnemyProperties.Effects)EnemyInfo.CurrentEffects[i], EnemyInfo.CurrentEffectsDuration[i]);
        }
    }

    [System.Serializable]
    public class SavedEnemyInfo{
        public float Health;

        public bool Chasing;
        public bool Roaming;
        public bool Idle;

        public int CurrentDirection;

        public List<int> CurrentEffects = new List<int>();
        public List<float> CurrentEffectsDuration = new List<float>();

    }
}
