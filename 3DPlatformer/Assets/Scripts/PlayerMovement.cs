using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("Player's speed when on ground.")]
    public float groundMoveSpeed = 5f;
    [Tooltip("Player's speed when in the air.")]
    public float inAirSpeed = 3f;
    [Tooltip("Force applied when the player jumps.")]
    public float jumpForce = 10f;
    [Tooltip("Time taken to smoothly rotate the player towards the movement direction.")]
    public float rotationSmoothTime = 0.05f;
    [Tooltip("Maximum number of jumps allowed.")]
    public int maxJumps = 2;

    [Header("Dash")]
    [Tooltip("Force applied when the player performs a dash.")]
    public float dashForce = 20f;
    [Tooltip("Cooldown time between consecutive dashes.")]
    public float dashCooldown = 2f;
    [Tooltip("Duration of the dash.")]
    public float dashDuration = 0.25f;

    [Space]

    [Header("References")]
    [Tooltip("This should be placed at the bottom of player.")]
    public Transform groundCheck;
    [Tooltip("The mask determining what is considered as ground for the player.")]
    public LayerMask groundMask;

    [Header("Events")]
    public GameEvent onPlayerDash;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private float currentVelocity;
    private float movementSpeed;
    private int jumpsLeft;
    private bool canDash = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");
    }

    private void Update()
    {
        MovePlayer();
        RotatePlayer();

        if (IsGrounded())
        {
            jumpsLeft = maxJumps;
            if (Input.GetButtonDown("Jump")) Jump();
            movementSpeed = groundMoveSpeed;
        }
        else
        {
            if (Input.GetButtonDown("Jump") && jumpsLeft > 1) Jump();
            movementSpeed = inAirSpeed;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) Dash();
    }

    /// <summary>
    /// Move the player based on input.
    /// </summary>
    private void MovePlayer()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 moveVelocity = moveDirection * movementSpeed;

        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
    }

    /// <summary>
    /// Rotate the player smoothly in the direction of movement.
    /// </summary>
    private void RotatePlayer()
    {
        var moveInput = new Vector3(horizontalInput, 0, verticalInput);
        if (moveInput.sqrMagnitude == 0) return;

        var forwardAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg;
        var smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, forwardAngle, ref currentVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
    }

    /// <summary>
    /// Make the player jump and reduce available jumps.
    /// </summary>
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpsLeft--;
    }

    /// <summary>
    /// Check if the player is grounded.
    /// </summary>
    /// <returns>True if the player is grounded, false otherwise.</returns>
    private bool IsGrounded()
    {
        bool isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);

        return isGrounded;
    }

    /// <summary>
    /// Initiate a dash if possible.
    /// </summary>
    private void Dash()
    {
        if (canDash)
        {
            StartCoroutine(PerformDash(transform.forward));
            canDash = false;
            onPlayerDash.Raise(this, dashCooldown);
            StartCoroutine(DashCooldown());
        }
    }

    /// <summary>
    /// Perform the dash action.
    /// </summary>
    /// <param name="dashDirection">Direction in which to dash.</param>
    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, dashDirection * dashForce, elapsedTime / dashDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = dashDirection * dashForce;
    }

    /// <summary>
    /// Apply a cooldown before the player can dash again.
    /// </summary>
    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
