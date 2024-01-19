using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables: Movement
    [Header("Movement")]
    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;

    public float speed;
    public float acceleration;

    private float currentSpeed;

    #endregion

    #region Variables: Gravity
    [Header("Gravity")]
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;

    #endregion

    #region Variables: Jumping
    [Header("Jumping")]
    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    public float groundCheckOffset = 1;
    public float groundCheckSize = 0.5F;
    public LayerMask groundLayer;

    #endregion

    #region Variables: Dash
    [Header("Dash")]
    public GameEvent onPlayerDash;
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 2f;
    private bool _isDashing;
    private bool _canDash = true;

    #endregion

    #region Variables: Ground Pound
    
    [Header("Ground Pound")]
    public float poundDuration = 5;
    public float poundSpeed = 5;
    public float poundCooldown = 2;
    public GameEvent onPlayerGroundPound;
    private bool _canPound = false;
    private bool _isPounding = false;
    private bool _poundOnCooldown = false;

    #endregion

    #region Animation

    private Animator _animator;

    #endregion

    [Header("Etc.")]
    public GameObject groundHitVFX;

    private float currentVelocity;
    private float rotationSmoothTime = 0.05f;
    [HideInInspector]public bool canMove = true;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!canMove)
        {
            _animator.SetTrigger("Teleporting");
            return;
        }

        RotatePlayer();
        ApplyMovement();
        ApplyGravity();

        if (!IsGrounded() && !_isPounding) _canPound = true;

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

        if (_isPounding) GroundPound();

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
        if (_numberOfJumps == 1) _animator.SetTrigger("DoubleJump");

        _numberOfJumps++;
        _velocity = jumpPower;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_canDash && _characterController.velocity.magnitude > 0.1f && canMove) _isDashing = true;
    }

    public void GroundPound(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_isPounding && _canPound && !IsGrounded()) _isPounding = true;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);
        _animator.SetTrigger("isLanded");
        _animator.SetBool("Jump", false);
        _numberOfJumps = 0;

        if (_isPounding && !_poundOnCooldown)
        {
            onPlayerGroundPound.Raise(this, poundCooldown);
            StartCoroutine(PoundCooldown());
        }

        groundHitVFX.SetActive(true);
    }

    //private bool IsGrounded() => _characterController.isGrounded;

    private bool IsGrounded()
    {
        var grouncheckPosition = transform.position - new Vector3(0, groundCheckOffset, 0);
        Collider[] hitColliders = new Collider[1];
        int numColliders = Physics.OverlapBoxNonAlloc(grouncheckPosition, new Vector3(groundCheckSize, groundCheckSize), hitColliders, Quaternion.identity, groundLayer);

        if (numColliders > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Dash()
    {
        if (_canDash && canMove)
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
        _animator.SetBool("Dash", true);
        _animator.SetTrigger("DashTrigger");
        
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
        _animator.SetBool("Dash", false);
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }

    private void GroundPound()
    {
        StartCoroutine(PoundCoroutine());
    }

    private IEnumerator PoundCoroutine()
    {
        if (!_canPound) yield return null;

        float timer = 0f;

        while (timer < poundDuration && _canPound && _isPounding)
        {
            _characterController.Move(Vector3.down * poundSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PoundCooldown()
    {
        _poundOnCooldown = true;
        _canPound = false;
        yield return new WaitForSeconds(poundCooldown);
        _canPound = true;
        _poundOnCooldown = false;
        _isPounding = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position - new Vector3(0, groundCheckOffset, 0), new Vector3(groundCheckSize, groundCheckSize, groundCheckSize));

        Gizmos.color = IsGrounded() ? Color.red : Color.green;
    }
}