using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SkiPlayerController : MonoBehaviour
{
    # region variables
    Rigidbody _rb;
    [SerializeField]
    Vector2 _moveDirection;
    private bool _inputJump;
    [SerializeField] private bool isGrounded;
    [SerializeField]
    private float baseSpeed,gravity,jumpHeight,jumpDelay,deceleration,alignmentRotationSpeed,raycastDistance,movementRotationSpeed,slopeSpeed,maxSpeed,driftSpeed;
    [SerializeField]
    GameObject playerCinCamera;

    private Vector3 _cameraForward, _cameraRight,_groundNormal;
    [SerializeField]
    Vector3 playerVelocity;
    
    private bool _isJumping=false;
    
    private RaycastHit _hitInfo;
    
    [Header("Visualizer")]
    [SerializeField] private LineRenderer velocityLineRenderer;
    [SerializeField] private LineRenderer groundHorizontalLineRenderer,forwardHorizontalLineRenderer,
        forwardLineRenderer,groundNormalLineRenderer;
    [SerializeField] private GameObject board,boardTip;

    private Vector2 _groundHorizontal, _forwardHorizontal,_playerDirection=new Vector2(0,0);
        
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

    private void Start()
    {
        _rb =  GetComponent<Rigidbody>();
        _playerDirection = transform.forward;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        AlignWithGround();
        if (isGrounded)
        {
            Jump();
        }
        
        //_playerYVelocity = _controller.velocity.y;
        UpdateCamera();
        Rotate();
        UpdateVelocity();
        UpdateVisualizer();
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

    void UpdateVelocity()
    {
        Vector3 slopeForce=_groundNormal.normalized;
        float comparedForce= -Vector3.Dot(slopeForce,Vector3.up);
        
        _groundHorizontal= new Vector2(_groundNormal.x,_groundNormal.z).normalized;
        //_forwardHorizontal= new Vector2(transform.forward.x,-transform.forward.z);
        _forwardHorizontal= new Vector2(boardTip.transform.position.x-board.transform.position.x,
            boardTip.transform.position.z-board.transform.position.z).normalized;
        float  directionalFactor = Vector2.Dot(_groundHorizontal.normalized,_forwardHorizontal.normalized);
        float slopeModifier = directionalFactor*comparedForce;
        
        _rb.AddForce( transform.forward * (slopeModifier * slopeSpeed),ForceMode.Force);//momentum
        
            //board force
            
            // if turning and you go over 180 to slowdown there needs to be an upwards force to counteract
            
            float turnPercent = Vector2.Dot(new Vector2(-board.transform.forward.x, -board.transform.forward.z).normalized,
                new Vector2(_groundHorizontal.x, _groundHorizontal.y));
            //this only works when we are not on flat ground
            
            Debug.Log("tp"+turnPercent);
            if(_playerDirection.y is > 10 and < 90)
                _rb.AddForce(board.transform.right * (1-turnPercent * driftSpeed) ,ForceMode.Force);
            else if (_playerDirection.y is > 270 and < 350)
            {
                _rb.AddForce(-board.transform.right * (1-turnPercent * driftSpeed) ,ForceMode.Force);
            }
            else if (_playerDirection.y is > 90 and < 180)
            {
                _rb.AddForce(-board.transform.right * (1-turnPercent * driftSpeed) ,ForceMode.Force);
            }
            else if (_playerDirection.y is > 180 and < 270)
            {
                _rb.AddForce(board.transform.right * (1-turnPercent * driftSpeed) ,ForceMode.Force);
            }
        
        
        //float force = (_playerDirection.y %180)/180;
        //_rb.AddForce(transform.right * (force * driftSpeed),ForceMode.Force);
        _rb.AddForce(new (0,gravity,0));//gravity
        Debug.Log(_rb.linearVelocity);
        //I need to keep track of his direction in a vector2
    }

    void Jump()
    {
        if (!_inputJump) return;
        if (_isJumping) return;
        _isJumping  = true;
        _rb.AddForce(Vector3.up * (jumpHeight * -gravity),ForceMode.Impulse); 
        StartCoroutine("JumpRoutine");
    }

    void Rotate()
    {
        _playerDirection +=  new Vector2(0,_moveDirection.x * movementRotationSpeed*Time.deltaTime);
        _playerDirection.y = _playerDirection.y % 360;
        if (_playerDirection.y < 0)
        {
            _playerDirection.y = 360 + _playerDirection.y;
        }
        board.transform.localEulerAngles = new Vector3(0,_playerDirection.y,0);
    }


    void AlignWithGround()
    {
        _groundNormal = GroundNormal().normal;
        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward,_groundNormal);
        Quaternion targetRotation = Quaternion.LookRotation(projectedForward,_groundNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignmentRotationSpeed * Time.deltaTime);
    }

    RaycastHit GroundNormal()
    { 
        isGrounded = Physics.Raycast(transform.position, Vector3.down*raycastDistance, out _hitInfo, raycastDistance, LayerMask.GetMask("Terrain"));
        return _hitInfo;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
        if (_hitInfo.collider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(_hitInfo.point, _hitInfo.normal);
        }
    }

    private void UpdateVisualizer()
    {
        Vector3 velocity = _rb.linearVelocity;
        velocityLineRenderer.SetPosition(1, new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z) * 6);
        groundHorizontalLineRenderer.SetPosition(1,new Vector3(_groundHorizontal.x,0,_groundHorizontal.y)*6);
        forwardHorizontalLineRenderer.SetPosition(1,new Vector3(_forwardHorizontal.x,0,_forwardHorizontal.y)*6);
        //forwardLineRenderer.SetPosition(1,transform.forward*6);
        forwardLineRenderer.SetPosition(1,-board.transform.forward*6);
        groundNormalLineRenderer.SetPosition(1,_groundNormal);
        
    }

    # region Timers

    IEnumerator JumpRoutine()
    {
        yield return  new WaitForSeconds(jumpDelay);
        _isJumping = false;
    }
    # endregion
}
