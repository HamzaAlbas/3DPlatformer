using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables: Movement

    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;

    public float speed;
    public float acceleration;

    private float currentSpeed;

    #endregion

    #region Variables: Gravity

    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;

    #endregion

    #region Variables: Jumping

    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;

    #endregion

    #region Variables: Dash

    public GameEvent onPlayerDash;
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 2f;
    private bool _isDashing;
    private bool _canDash = true;

    #endregion

    #region Animation

    private Animator _animator;

    #endregion

    private float currentVelocity;
    private float rotationSmoothTime = 0.05f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        RotatePlayer();
        ApplyGravity();
        ApplyMovement();


        var velocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

        if(velocity.magnitude <= 0.1f)
        {
            _animator.SetBool("isWalking", false);
        }
        else
        {
            _animator.SetBool("isWalking", true);
        }

        if (_isDashing) Dash();
        _animator.SetBool("isGrounded", IsGrounded());
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }

        _direction.y = _velocity;
    }

    private void RotatePlayer()
    {
        //var moveInput = new Vector3(_currentMovement.x, 0f, _currentMovement.z);
        if (_input.sqrMagnitude == 0) return;

        var forwardAngle = Mathf.Atan2(_input.x, _input.y) * Mathf.Rad2Deg;
        //_direction = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * new Vector3(_input.x, 0.0f, _input.y);
        var smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, forwardAngle, ref currentVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
    }

    private void ApplyMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);

        _characterController.Move(_direction * currentSpeed * Time.deltaTime);

    }

    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;
        if (_numberOfJumps == 0) StartCoroutine(WaitForLanding());

        if (_numberOfJumps == 0) _animator.SetBool("Jump", true);
        if (_numberOfJumps == 1) _animator.SetBool("DoubleJump", true);

        _numberOfJumps++;
        _velocity = jumpPower;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_canDash) _isDashing = true;
        

    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        _animator.SetTrigger("isLanded");
        _animator.SetBool("Jump", false);
        _animator.SetBool("DoubleJump", false);
        _numberOfJumps = 0;
    }

    private bool IsGrounded() => _characterController.isGrounded;

    private void Dash()
    {
        if (_canDash)
        {
            StartCoroutine(DashCoroutine());
            _canDash = false;
            _isDashing = false;
            onPlayerDash.Raise(this, dashCooldown);
            StartCoroutine(DashCooldown());
        }

    }

    private IEnumerator DashCoroutine()
    {
        if (!_canDash) yield return null;
        float timer = 0f;
        _animator.SetTrigger("Dash");
        
        while (timer < dashDuration)
        {
            _characterController.Move(transform.forward * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DashCooldown()
    {
        _canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }
}