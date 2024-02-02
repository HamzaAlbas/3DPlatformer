using System.Collections;
using UnityEngine;

public class PlayerGroundPound : MonoBehaviour
{
    public float groundPoundForce = 20f;
    public float cooldownTime = 2f;
    public GameObject groundPoundVFX;
    public GameEvent onPlayerGroundPound;

    private CharacterController characterController;
    private bool isGroundPounding = false;
    private bool canPound = true;
    private PlayerMovement _playerMovement;
    private Animator _animator;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        _playerMovement = GetComponent<PlayerMovement>(); 
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (InputManager.Instance.GetGroundPound() && !isGroundPounding && canPound && _playerMovement.canMove)
        {
            canPound = false;
            StartCoroutine(GroundPound());
        }
    }

    IEnumerator GroundPound()
    {
        _playerMovement.canMove = false;
        isGroundPounding = true;

        while (!characterController.isGrounded)
        {
            Vector3 groundPoundVelocity = Vector3.down * groundPoundForce;
            characterController.Move(groundPoundVelocity * Time.deltaTime);
            yield return null;
        }

        onPlayerGroundPound.Raise(this, cooldownTime);
        _animator.SetTrigger("HardLand");
        groundPoundVFX.SetActive(true);
        StartCoroutine(Cooldown());

        isGroundPounding = false;
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canPound = true;
    }

    public void PoundControlsStart()
    {
        _playerMovement.canMove = false;
    }

    public void PoundControlsEnd()
    {
        _playerMovement.canMove = true;
    }
}
