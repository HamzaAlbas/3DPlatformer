using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public GameEvent onPlayerDash;
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 2f;
    private bool _canDash = true;
    private MeshTrail meshTrail;
    private PlayerMovement _playerMovement;
    private CharacterController _characterController;
    private Animator _animator;

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        meshTrail = GetComponent<MeshTrail>();
    }

    private void Update()
    {
        if (InputManager.Instance.GetDash())
        {
            Dash();
        }
    }

    private void Dash()
    {
        if(_canDash && _playerMovement.canMove && _playerMovement.CharacterVelocity.magnitude > Mathf.Epsilon)
        {
            _animator.SetTrigger("Dash");
            _animator.SetBool("DashEnd", false);
            StartCoroutine(DashCoroutine());
            meshTrail.StartTrail(dashDuration);
            _canDash = false;
            onPlayerDash.Raise(this, dashCooldown);
        }
    }

    private IEnumerator DashCoroutine()
    {
        if (!_canDash) yield return null;
        float timer = 0f;
        var oldGravity = _playerMovement.GravityMultiplier;
        _playerMovement.GravityMultiplier = 0;
        _playerMovement.Velocity = 0;

        _playerMovement.canMove = false;

        if (_playerMovement.TargetAngle == 90) transform.rotation = Quaternion.Euler(transform.rotation.x, 90f, transform.rotation.z);
        if (_playerMovement.TargetAngle == -90) transform.rotation = Quaternion.Euler(transform.rotation.x, 270f, transform.rotation.z);


        while (timer < dashDuration)
        {
            _characterController.Move(transform.forward * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        _playerMovement.GravityMultiplier = oldGravity;
        _playerMovement.canMove = true;
        _animator.SetBool("DashEnd", true);
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        _canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }
}
