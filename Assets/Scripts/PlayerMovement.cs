using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This PlayerMovement Script gives no freedom of movement after jumping
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private enum MovementType { NAC, LAC, FAC };
    private enum DashType { Left, Right}

    #region Serialized Fields
    [Header("Speed Values")]
    [SerializeField]
    private float minRunSpeed;

    [SerializeField]
    [Range(0,1)]
    private float accelerationRate;

    [SerializeField]
    private float maxRunSpeed;

    [SerializeField]
    private float jumpSpeed;

    [SerializeField]
    private float doubleJumpSpeed;

    [SerializeField]
    private float maxJumpSpeed;

    [SerializeField]
    [Tooltip("Used for LAC movement, is the horizontal movement speed when the player is airborne")]
    [Range(0,1)]
    private float airSpeed;

    [SerializeField]
    [Range(0,1)]
    private float clingGravityScale;

    [SerializeField]
    private float wallJumpHorizontalSpeed;

    [Header("Dash Values")]
    [SerializeField]
    private float dashSpeed;

    [SerializeField]
    private float dashDuration;

    [SerializeField]
    private float dashEndLag;

    [SerializeField]
    private float dashCooldown;

    [Header("Misc")]
    [SerializeField]
    private MovementType playerMovementType;

    [SerializeField]
    private WallCheck leftCheck, rightCheck;

    [SerializeField]
    private bool canDoubleJump = false;

    [SerializeField]
    private static int maxJumps = 2;

    [SerializeField]
    private float wallJumpRecoveryTime;

    [SerializeField]
    private PhysicsMaterial2D Moving;

    [SerializeField]
    private PhysicsMaterial2D Stopped;

    #endregion
    private Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    private GroundCheck groundCheck;

    private DashType dashType;

    private bool IsInAir, IsClingingLeft, IsClingingRight, CanMoveLeft = true, CanMoveRight = true, CanMove = true, CanDash = true, FacingRight = true;
    private int Jumps = maxJumps;
    private float SpeedGain = 0;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        groundCheck = FindObjectOfType<GroundCheck>();
    }

    void Update()
    {
        IsInAir = groundCheck.IsInAir();
        IsClingingLeft = leftCheck.IsClinging() && Input.GetAxis("Horizontal") < 0;
        IsClingingRight = rightCheck.IsClinging() && Input.GetAxis("Horizontal") > 0;
        if(!IsInAir)
        {
            Jumps = maxJumps;
        }
        if (Input.GetAxisRaw("Horizontal") == 0 || IsClingingLeft || IsClingingRight)
            SpeedGain = 0;
        else if (SpeedGain == 1)
        {

        }
        else
            SpeedGain += accelerationRate;
        HandleDirection();
        if(CanMove)
        {
            if (playerMovementType == MovementType.NAC)
                NACMovement();
            else if (playerMovementType == MovementType.LAC)
                LACMovement();
            else if (playerMovementType == MovementType.FAC)
                FACMovement();

            CheckForCling();
            GetJumpInput();
            GetDashInput();
        }
    }

    private void HandleDirection()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
            FacingRight = true;
        else if (Input.GetAxisRaw("Horizontal") < 0)
            FacingRight = false;
    }

    private void NACMovement()
    {
        if (Input.GetButton("Horizontal") && !IsInAir)
        {
            boxCollider.sharedMaterial = Moving;
            if (Input.GetAxis("Horizontal") > 0 && CanMoveRight || Input.GetAxis("Horizontal") < 0 && CanMoveLeft)
                rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * (minRunSpeed + (maxRunSpeed - minRunSpeed) * SpeedGain), rigidbody.velocity.y);
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxRunSpeed, maxRunSpeed);
            rigidbody.velocity = velocity;
        }
        else
        {
            boxCollider.sharedMaterial = Stopped;
        }
    }

    private void LACMovement()
    {
        if (Input.GetButton("Horizontal"))
        {
            boxCollider.sharedMaterial = Moving;
            if (Input.GetAxis("Horizontal") > 0 && CanMoveRight || Input.GetAxis("Horizontal") < 0 && CanMoveLeft)
            {
                if (!IsInAir)
                    rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * (minRunSpeed + (maxRunSpeed - minRunSpeed) * SpeedGain), rigidbody.velocity.y);
                else
                    rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * airSpeed * (minRunSpeed + (maxRunSpeed - minRunSpeed) * SpeedGain), rigidbody.velocity.y);
            }
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxRunSpeed, maxRunSpeed);
            rigidbody.velocity = velocity;
        }
        else
        {
            boxCollider.sharedMaterial = Stopped;
        }
    }

    private void FACMovement()
    {
        if (Input.GetButton("Horizontal"))
        {
            boxCollider.sharedMaterial = Moving;
            if(Input.GetAxis("Horizontal") > 0 && CanMoveRight || Input.GetAxis("Horizontal") < 0 && CanMoveLeft)
                rigidbody.velocity = new Vector2(Input.GetAxis("Horizontal") * (minRunSpeed + (maxRunSpeed - minRunSpeed) * SpeedGain), rigidbody.velocity.y);
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxRunSpeed, maxRunSpeed);
            rigidbody.velocity = velocity;
        }
        else
        {
            boxCollider.sharedMaterial = Stopped;
        }
    }

    private void CheckForCling()
    {
        if(IsClingingLeft || IsClingingRight)
        {
            rigidbody.gravityScale = clingGravityScale; 
        }
        else
        {
            rigidbody.gravityScale = 1;
        }
    }

    private void GetJumpInput()
    {
        if(canDoubleJump)
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (IsClingingLeft)
                {
                    Jumps = maxJumps;
                    rigidbody.velocity = new Vector2(wallJumpHorizontalSpeed, doubleJumpSpeed);
                    Vector2 velocity = rigidbody.velocity;
                    velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                    rigidbody.velocity = velocity;
                    StartCoroutine("PreventLeftMovement");
                    Jumps--;
                }
                else if (IsClingingRight)
                {
                    Jumps = maxJumps;
                    rigidbody.velocity = new Vector2(-wallJumpHorizontalSpeed, doubleJumpSpeed);
                    Vector2 velocity = rigidbody.velocity;
                    velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                    rigidbody.velocity = velocity;
                    StartCoroutine("PreventRightMovement");
                    Jumps--;
                }
                else if (!IsInAir)
                {
                    rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpSpeed);
                    Vector2 velocity = rigidbody.velocity;
                    velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                    rigidbody.velocity = velocity;
                    Jumps--;
                }
                else if (IsInAir && Jumps > 0)
                {
                    rigidbody.velocity = new Vector2(rigidbody.velocity.x, doubleJumpSpeed);
                    Vector2 velocity = rigidbody.velocity;
                    velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                    rigidbody.velocity = velocity;
                    Jumps--;
                }
            }
            if (Input.GetButtonUp("Jump") && rigidbody.velocity.y > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && !IsInAir)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpSpeed);
                Vector2 velocity = rigidbody.velocity;
                velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                rigidbody.velocity = velocity;
            }
            if (Input.GetButtonUp("Jump") && rigidbody.velocity.y > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            }
        }
    }

    private void GetDashInput()
    {
        if(CanDash)
        {
            if (Input.GetButtonDown("DashLeft") && !IsClingingLeft)
            {
                dashType = DashType.Left;
                StartCoroutine("Dash");
            }
            else if(Input.GetButtonDown("DashRight") && !IsClingingRight)
            {
                dashType = DashType.Right;
                StartCoroutine("Dash");
            }
        }
    }

    private IEnumerator Dash()
    {
        CanMove = false;
        CanDash = false;
        rigidbody.gravityScale = 0;
        rigidbody.velocity = new Vector2(0, 0);
        int direction;
        if (FacingRight)
            direction = 1;
        else
            direction = -1;

        if (dashType == DashType.Right)
            rigidbody.velocity = Vector2.right * dashSpeed;
        else if (dashType == DashType.Left)
            rigidbody.velocity = Vector2.left * dashSpeed;
        for(float timer = dashDuration; timer >= 0; timer -= Time.deltaTime)
        {
            if(dashType == DashType.Left && Input.GetButtonDown("DashRight") || dashType == DashType.Right && Input.GetButtonDown("DashLeft"))
            {
                rigidbody.velocity = new Vector2(0, 0);
                break;
            }
            yield return null;
        }
        rigidbody.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(dashEndLag);
        CanMove = true;
        rigidbody.gravityScale = 1;
        yield return new WaitForSeconds(dashCooldown);
        CanDash = true;
    }

    private IEnumerator PreventLeftMovement()
    {
        CanMoveLeft = false;
        yield return new WaitForSeconds(wallJumpRecoveryTime);
        CanMoveLeft = true;
    }

    private IEnumerator PreventRightMovement()
    {
        CanMoveRight = false;
        yield return new WaitForSeconds(wallJumpRecoveryTime);
        CanMoveRight = true;
    }
}
