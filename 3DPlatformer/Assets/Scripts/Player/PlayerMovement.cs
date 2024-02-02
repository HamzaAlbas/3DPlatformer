using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region MOVEMENT

    [Header("Movement")]
    public float speed;
    public float acceleration;
    public bool canMove = true;

    private float currentSpeed;
    private Vector2 _input;
    private Vector3 _direction;

    #endregion

    #region GRAVITY

    [Header("Gravity")]

    private float _gravity = -9.81f;
    private float _velocity;
    [SerializeField] private float gravityMultiplier = 3.0f;


    #endregion

    #region ROTATION

    private float rotationSmoothTime = 0.05f;
    private float targetAngle;
    private float currentVelocity;

    #endregion

    #region JUMPING

    [Header("Jumping")]
    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    private Vector3 jumpStartPos;
    public GameObject groundHitVFX;

    #endregion

    #region GROUND CHECK

    [Header("Ground Check")]
    public float groundCheckOffset = 1;
    public float groundCheckSize = 0.5F;
    public LayerMask groundLayer;

    #endregion

    #region OTHER STUFF

    private Animator _animator;
    private CharacterController _characterController;
    private float sqrInput;
    private bool isWalking;
    private bool isTeleporting;

    public Vector3 CharacterVelocity
    {
        get { return _characterController.velocity; }
    }

    public float GravityMultiplier
    {
        get { return gravityMultiplier; }
        set { gravityMultiplier = value; }
    }

    public float Velocity
    {
        get { return _velocity; }
        set { _velocity = value; }
    }

    public float TargetAngle
    {
        get { return targetAngle; }
        set { value = targetAngle; }
    }

    #endregion

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!canMove) return;
        OutOfBoundsCheck();

        if (InputManager.Instance.GetJump())
        {
            HandleJumping();
        }

        ApplyGravity();
        HandleRotation();
        HandleMovement();

        //Animations
        if (!isTeleporting)
        {
            _animator.SetBool("IsGrounded", IsGrounded());
            _animator.SetFloat("Input", _input.sqrMagnitude);
            _animator.SetBool("IsWalking", isWalking);
        }
        

    }

    private void HandleRotation()
    {
        if (sqrInput < Mathf.Epsilon) return;

        targetAngle = Mathf.Atan2(_input.x, _input.y) * Mathf.Rad2Deg;
        var smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
    }

    private void HandleMovement()
    {
        _input = InputManager.Instance.GetMovement();
        sqrInput = _input.sqrMagnitude;
        _direction = new Vector3(_input.x, _direction.y, _input.y);
        currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
        _characterController.Move(_direction * currentSpeed * Time.deltaTime);

        var xVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);
        isWalking = xVelocity.sqrMagnitude > Mathf.Epsilon;
    }

    private void HandleJumping()
    {
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;

        maxNumberOfJumps = IsGrounded() ? 2 : 1;

        if (_numberOfJumps == 0 && maxNumberOfJumps == 2) _animator.SetTrigger("Jump");
        if (_numberOfJumps == 1 || maxNumberOfJumps == 1) _animator.SetTrigger("DoubleJump");
        if (_numberOfJumps == 0) StartCoroutine(WaitForLanding());

        _numberOfJumps++;
        _velocity = jumpPower;
        jumpStartPos = transform.position;
    }

    private void ApplyGravity()
    {
        if(IsGrounded() && _velocity < 0)
        {
            _velocity = -1f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
        _direction.y = _velocity;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);

        _numberOfJumps = 0;

        if (transform.position.y <= jumpStartPos.y)
        {
            Instantiate(groundHitVFX, transform.position, Quaternion.Euler(-90, 0, 0));
        }
    }

    public bool IsGrounded()
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

    private void OutOfBoundsCheck()
    {
        if(transform.position.y < -10)
        {
            transform.position = new Vector3(0, 1.75f, 0);
        }
    }

    private void OnDrawGizmos()
    {
        if (IsGrounded())
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawCube(transform.position - new Vector3(0, groundCheckOffset, 0), new Vector3(groundCheckSize, groundCheckSize, groundCheckSize));
    }

    public void TeleportStart()
    {
        isTeleporting = true;
        _animator.SetBool("IsWalking", false);
        canMove = false;
    }

    public void TeleportEnd()
    {
        isTeleporting = false;
        canMove = true;
    }
}
