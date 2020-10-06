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
    private float speed;

    [SerializeField]
    private float jumpSpeed;

    [SerializeField]
    private float doubleJumpSpeed;

    [SerializeField]
    private float maxSpeed;

    [SerializeField]
    private float maxJumpSpeed;

    [SerializeField]
    [Tooltip("Used for LAC movement, is the horizontal movement speed when the player is airborne")]
    private float airSpeed;

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
    private bool canDoubleJump = false;

    [SerializeField]
    private PhysicsMaterial2D Moving;

    [SerializeField]
    private PhysicsMaterial2D Stopped;

    #endregion
    private Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    private GroundCheck groundCheck;

    private DashType dashType;

    private bool isInAir, CanMove = true, CanDash = true, FacingRight = true;
    private int Jumps = 2;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        groundCheck = FindObjectOfType<GroundCheck>();
    }

    void Update()
    {
        isInAir = groundCheck.IsInAir();
        if(!isInAir)
        {
            Jumps = 2;
        }
        HandleDirection();
        if(CanMove)
        {
            if (playerMovementType == MovementType.NAC)
                NACMovement();
            else if (playerMovementType == MovementType.LAC)
                LACMovement();
            else if (playerMovementType == MovementType.FAC)
                FACMovement();

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
        if (Input.GetButton("Horizontal") && !isInAir)
        {
            boxCollider.sharedMaterial = Moving;
            rigidbody.velocity = new Vector2(speed * Input.GetAxis("Horizontal"), rigidbody.velocity.y);
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
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
            if (!isInAir)
                rigidbody.velocity = new Vector2(speed * Input.GetAxis("Horizontal"), rigidbody.velocity.y);
            else
                rigidbody.velocity = new Vector2(airSpeed * Input.GetAxis("Horizontal"), rigidbody.velocity.y);
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
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
            rigidbody.velocity = new Vector2(speed * Input.GetAxis("Horizontal"), rigidbody.velocity.y);
            Vector2 velocity = rigidbody.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
            rigidbody.velocity = velocity;
        }
        else
        {
            boxCollider.sharedMaterial = Stopped;
        }
    }

    private void GetJumpInput()
    {
        if(canDoubleJump)
        {
            if (Input.GetButtonDown("Jump") && !isInAir)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpSpeed);
                Vector2 velocity = rigidbody.velocity;
                velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                rigidbody.velocity = velocity;
                Jumps--;
            }
            else if (Input.GetButtonDown("Jump") && isInAir && Jumps > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, doubleJumpSpeed);
                Vector2 velocity = rigidbody.velocity;
                velocity.y = Mathf.Clamp(velocity.y, Mathf.NegativeInfinity, maxJumpSpeed);
                rigidbody.velocity = velocity;
                Jumps = 0;
            }
            if (Input.GetButtonUp("Jump") && rigidbody.velocity.y > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && !isInAir)
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
            if (Input.GetButtonDown("DashLeft"))
            {
                dashType = DashType.Left;
                StartCoroutine("Dash");
            }
            else if(Input.GetButtonDown("DashRight"))
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
                //if(dashType == DashType.Left && Input.GetButtonDown("DashRight") || dashType == DashType.Right && Input.GetButtonDown("DashLeft"))
                //{
                //    if (dashType == DashType.Left)
                //        rigidbody.velocity = Vector2.right * dashSpeed;
                //    else if (dashType == DashType.Right)
                //        rigidbody.velocity = Vector2.left * dashSpeed;
                //    yield return new WaitForSeconds(timer);
                //}
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
}
