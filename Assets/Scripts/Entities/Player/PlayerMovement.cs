using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public float WalkSpeed;

    [Header("")]
    
    [Header("Components")]
    public Rigidbody2D rig;
    public Animator animator;
    public UIManager UIManager;

    [Header("Debug")]
    public Vector2 input;
    public Vector2 DirectionalSpeed;
    public float Speed;
    public State CurrentState;
    public Direction CurrentDirection;

    void Start()
    {
        
    }

    void Update()
    {
        Vector2 vec = new Vector2(Mathf.Abs(rig.velocity.x), Mathf.Abs(rig.velocity.y));

        if (vec.x > vec.y){
            if (rig.velocity.x > 0){
                CurrentDirection = Direction.Right;
            }else{
                CurrentDirection = Direction.Left;
            }
        }else{
            if (rig.velocity.y > 0){
                CurrentDirection = Direction.Up;
            }else{
                CurrentDirection = Direction.Down;
            }
        }

        if (input != Vector2.zero){
            CurrentState = State.Walking;
        }else{
            CurrentState = State.Idle;
        }

        if (UIManager.AMenuIsOpened()){
            CurrentState = State.InUI;
        }

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        

        switch (CurrentState){
            case State.Walking:
                Speed = WalkSpeed;

                DirectionalSpeed = input;
                if (input.magnitude > 1f){
                    DirectionalSpeed = input.normalized;
                }

                switch (CurrentDirection){
                    case Direction.Up:
                        animator.SetInteger("WalkDiraction", 0);
                        break;
                    case Direction.Down:
                        animator.SetInteger("WalkDiraction", 1);
                        break;
                    case Direction.Left:
                        animator.SetInteger("WalkDiraction", 2);
                        transform.rotation = Quaternion.Euler(0,180,0);
                        break;
                    default:
                        animator.SetInteger("WalkDiraction", 3);
                        transform.rotation = Quaternion.Euler(0,0,0);
                        break;
                }

                animator.SetBool("Walking", true);
                break;
            default:
                Speed = 0;
                animator.SetBool("Walking", false);
                break;
        }

        
    }

    void FixedUpdate()
    {   
        if (!GameUtils.OverASpeedInDirection(rig.velocity, DirectionalSpeed, Speed)){
            rig.AddForce(DirectionalSpeed * Speed, ForceMode2D.Force);
        }
    }

    public enum State{
        Walking,
        Running,
        InUI,
        Rolling,
        Idle,
        Stunned
    }
    public enum Direction{
        Up,
        Down,
        Left,
        Right
    }
}
