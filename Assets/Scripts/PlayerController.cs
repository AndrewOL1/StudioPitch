using System.Collections;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    # region variables
    CharacterController _controller;
    [SerializeField]
    Vector2 _moveDirection;
    private bool _inputJump;
    [SerializeField]
    private float _speed,gravity,jumpHeight,jumpDelay;
    [SerializeField]
    GameObject playerCinCamera;

    private Vector3 _cameraForward, _cameraRight;
    [SerializeField]
    private float _playerYVelocity;
    
    private bool _isJumping=false,_grounded;
    # endregion
    
    # region InputHandlers

    public void ReadJump(InputAction.CallbackContext context)
    {
        _inputJump = context.ReadValueAsButton();
    }
    public void ReadMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>().normalized;
    }
    # endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnSpawned()
    {
        _controller =  GetComponent<CharacterController>();
    }
    
    void FixedUpdate()
    {
        _grounded = _controller.isGrounded;

        if (_grounded)
        {
            if (_playerYVelocity < -2f) 
                _playerYVelocity = -2f;
        }
        
        //_playerYVelocity = _controller.velocity.y;
        UpdateCamera();
        Jump();
        Vector3 adjustedDirection = _cameraForward * _moveDirection.y + _cameraRight * _moveDirection.x;
        _playerYVelocity += gravity * Time.deltaTime; //gravity
        Vector3 finalMove = adjustedDirection * _speed + Vector3.up * _playerYVelocity;
        _controller.Move(finalMove *Time.deltaTime);
    }

    private void UpdateCamera()
    {
        _cameraForward=playerCinCamera.transform.forward;
        _cameraRight=playerCinCamera.transform.right;
        _cameraForward.y = 0;
        _cameraRight.y = 0;
        _cameraForward.Normalize();
        _cameraRight.Normalize();
    }

    void Jump()
    {
        if (!_inputJump) return;
        if (_isJumping) return;
        _isJumping  = true;
        _playerYVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
        StartCoroutine("JumpRoutine");
    }
    # region Timers

    IEnumerator JumpRoutine()
    {
        yield return  new WaitForSeconds(jumpDelay);
        _isJumping = false;
    }
    # endregion
}
