using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public float WalkSpeed;
    public float RunSpeed;
    [Header("Components")]
    public Rigidbody2D rig;
    public Animator animator;
    public GameObject FootParticles;
    [Header("Debug")]
    public Vector2 input;
    public Vector2 DirectionalSpeed;
    public float Speed;
    public Dictionary<State, bool> CurrentStates = new Dictionary<State, bool>{
        {State.Walking, false},
        {State.Running, false},
        {State.InUI, false},
        {State.Rolling, false},
        {State.Idle, false},
        {State.Dead, false},
        {State.InAnimation, false},
        {State.InShallowWater, false},
        {State.InBoat, false},

    };
    public Dictionary<Effects, float> CurrentEffects = new Dictionary<Effects, float>();
    public Direction CurrentDirection;
    void Update(){
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        //Change Direction Based on where you're walking
        Vector2 TempInput = new Vector2(Mathf.Abs(input.x), Mathf.Abs(input.y));

        if (TempInput.x > TempInput.y){
            if (input.x > 0){
                CurrentDirection = Direction.Right;
            }else{
                CurrentDirection = Direction.Left;
            }
        }else{
            if (input.y > 0){
                CurrentDirection = Direction.Up;
            }else{
                CurrentDirection = Direction.Down;
            }
        }

        DirectionalSpeed = input;
        if (input.magnitude > 1f)
            DirectionalSpeed = input.normalized;

        if (FootParticles.activeSelf != CurrentStates[State.Running])
            FootParticles.SetActive(CurrentStates[State.Running]);

        SetUpStates();

        if (CurrentStates[State.Walking] == true && !CurrentStates[State.InUI] && !CurrentStates[State.Dead]){
            Speed = CurrentStates[State.Running]? RunSpeed : WalkSpeed;
            if (CurrentStates[State.InShallowWater]) Speed -= CurrentStates[State.Running]? 1 : 1.5f;
            animator.speed = CurrentStates[State.Running]? RunSpeed/WalkSpeed : 1;

            switch (CurrentDirection){
                case Direction.Up:
                    animator.SetInteger("WalkingDirection", 0);
                    FootParticles.transform.rotation = Quaternion.Euler(90,-90,0);
                    break;
                case Direction.Down:
                    animator.SetInteger("WalkingDirection", 1);
                    FootParticles.transform.rotation = Quaternion.Euler(-90,90,0);
                    break;
                case Direction.Left:
                    animator.SetInteger("WalkingDirection", 2);
                    FootParticles.transform.rotation = Quaternion.Euler(-30,90,0);
                    GameServices.GlobalVariables.Player.SpriteRenderer.transform.rotation = Quaternion.Euler(0,180,0);
                    break;
                default:
                    animator.SetInteger("WalkingDirection", 3);
                    FootParticles.transform.rotation = Quaternion.Euler(-30,-90,0);
                    GameServices.GlobalVariables.Player.SpriteRenderer.transform.rotation = Quaternion.Euler(0,0,0);
                    break;
            }

            animator.SetBool("Walking", true);

        }else{
            Speed = 0;
            animator.speed = 1f;
            animator.SetBool("Walking", false);
        }

        
    }

    void FixedUpdate()
    {   
        if (!GameUtils.OverASpeedInDirection(rig.velocity, DirectionalSpeed, Speed))
            rig.velocity += DirectionalSpeed * Speed;
    }

    private void SetUpStates(){
        CurrentStates[State.InUI] = GameServices.UI.AMenuIsOpened();
        CurrentStates[State.Walking] = input != Vector2.zero;
        CurrentStates[State.Idle] = !CurrentStates[State.Walking] && !CurrentEffects.ContainsKey(Effects.Stunned);
        CurrentStates[State.Running] = Input.GetKey(KeyCode.LeftShift);

        HashSet<Effects> EffectsToDelete = new HashSet<Effects>();

        // Get a temporary list of keys to iterate safely
        List<Effects> keys = new List<Effects>(CurrentEffects.Keys);

        foreach (var key in keys) {
            // Modify the value
            CurrentEffects[key] -= Time.deltaTime;

            // Check if it should be deleted
            if (CurrentEffects[key] <= 0) {
                EffectsToDelete.Add(key);
            }
        }

        foreach(Effects key in EffectsToDelete)
            CurrentEffects.Remove(key);
    }

    public enum State{
        Walking,
        Running,
        InUI,
        Rolling,
        Idle,
        InAnimation,
        InShallowWater,
        InBoat,
        Dead
    }

    public enum Effects{
        Stunned,
    }
    public enum Direction{
        Up,
        Down,
        Left,
        Right
    }
}
