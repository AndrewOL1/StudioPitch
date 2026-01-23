using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SnowPlayerController : MonoBehaviour
{
    # region variables
    CharacterController _controller;
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
        _controller =  GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        AlignWithGround();
        if (isGrounded)
        {
            if (playerVelocity.y < -2f) 
                playerVelocity.y = -2f;
        }
        
        //_playerYVelocity = _controller.velocity.y;
        UpdateCamera();
        //Jump();
        Rotate();
        Move();
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
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        StartCoroutine("JumpRoutine");
    }

    void Rotate()
    {
        transform.rotation *= Quaternion.Euler(0,_moveDirection.x * movementRotationSpeed*Time.deltaTime,0);
    }

    void Move()
    {
        Vector3 finalMove=Vector3.zero;
        //apply deceleration
        
        //if the velocity is below base speed
        //check if there is a movement input
        //set speed to base speed
        if(baseSpeed>playerVelocity.magnitude)
            if (_moveDirection.magnitude > 0.2)
                playerVelocity = new Vector3(0f,0f,_moveDirection.y)*baseSpeed;
        
        //compare the ground normal to vector.up to find the slope***
        //normalize the ground normals x and y in a vector2 to find the direction of the slope
        //compare the direction of the slope to our forward vector (apply more speed the closer they align)
        //scale the speed with the slope
        //add value to current velocity
        
        
        //slope Factor
        Vector3 slopeForce=_groundNormal.normalized;
        float comparedForce= -Vector3.Dot(slopeForce,Vector3.up);
        comparedForce=comparedForce*slopeSpeed;
        
        //DirectionalFactor
        //need to find a way to look
        Vector2 groundHorizontal= new Vector2(_groundNormal.x,_groundNormal.z);
        Vector2 forwardHorizontal= new Vector2(transform.forward.x,transform.forward.z);
        float  directionalFactor = Vector2.Dot(groundHorizontal.normalized,forwardHorizontal.normalized);
        float slopeModifier = directionalFactor*comparedForce;
        if (playerVelocity.x == 0 && playerVelocity.z == 0)
        {
            playerVelocity = transform.forward*driftSpeed;
        }
        playerVelocity=new Vector3(playerVelocity.x*slopeModifier,playerVelocity.y + gravity,
            playerVelocity.z*slopeModifier);
        //playerVelocity.y += gravity * Time.deltaTime; //gravity
        //finalMove+=Vector3.up*gravity * Time.deltaTime;
        //move the final vector
        
        _controller.Move(playerVelocity*Time.deltaTime);
        UpdateVisualizer();
        
        
    }

    float CalAccleration( )
    {
        float accleration = playerVelocity.magnitude;
        return accleration;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_hitInfo.point, _hitInfo.normal * raycastDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * raycastDistance);
    }

    private void UpdateVisualizer()
    {
        velocityLineRenderer.SetPosition(1, playerVelocity);
    }

    # region Timers

    IEnumerator JumpRoutine()
    {
        yield return  new WaitForSeconds(jumpDelay);
        _isJumping = false;
    }
    # endregion
}
