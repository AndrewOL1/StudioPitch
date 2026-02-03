using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SSXPlayerController : MonoBehaviour
{
    # region variables
    [Header("References")]
    public Transform cameraTransform;
    
    [Header("Movement Settings")]
    [SerializeField] private float _minSpeed = 5f;
    [SerializeField] private float _maxSpeed = 45f,_downhillAccel = 25f,_friction = 1f;
    [SerializeField] float maxRideableSlope = 55f;
    [SerializeField] float minSpeedSlopeLimit = 10f;
    
    [Header("Turning")]
    [SerializeField]private float _turnRate = 120f; // degrees per second
    [SerializeField]private float _airTurnRate = 35f;
    [SerializeField]private float _speedTurnScale = 0.015f; // higher = wider turns at speed
    [SerializeField]private float _downhillBias = 15f;


    [Header("Gravity")]
    [SerializeField]private float _gravity = -25f;
    [SerializeField]private float _airGravityScale = 0.6f;
    
    [Header("Jump")]
    [SerializeField]private float jumpForce = 10f;
    [SerializeField]private float jumpGraceTime = 0.15f;

    [Header("Grounding")]
    [SerializeField]private float _groundCheckDistance = 1.5f;
    private float _groundStickForce = 2f,_lastGroundedTime;


    [Header("Landing Boost")]
    [SerializeField]private float _landingBoost = 6f;


    [Header("Smoothing")]
    [SerializeField]private float _inputSmoothTime = 0.1f;

    [Header("Editor")] [SerializeField] private GameObject board;
    
    [SerializeField]private RaycastHit _hitInfo;
    
    public float _speed;
    public Vector3 velocity;
    float _verticalVelocity;


    Vector3 _forwardDir;
    Vector3 _groundNormal;


    bool _isGrounded;
    bool _wasGrounded;


    float _smoothInput;
    float _smoothInputVel;
    
    CharacterController _controller;
    
    private Vector2 _moveDirection;
    private bool _inputJump,_jumpedThisFrame,_suppressHorizontalThisFrame;
    
    float _lastLandingTime;
    float _landingCooldown = 0.1f;

    private bool _isPaused;
    
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
        _controller = GetComponent<CharacterController>();
        _speed = _minSpeed;
        _forwardDir = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if(_isPaused)return;
        HandleInput();
        GroundCheck();
        HandleJump();
        
        if (_isGrounded)
            GroundMovement();
        else AirMovement();
        
        _wasGrounded = _isGrounded;
        _jumpedThisFrame = false;
        _suppressHorizontalThisFrame = false;
    }

    private void HandleJump()
    {
        if (!_inputJump || !(Time.time - _lastGroundedTime <= jumpGraceTime)) return;
        //_controller.Move(Vector3.up * 0.05f);
        _verticalVelocity = jumpForce + Mathf.Clamp(_speed * 0.05f, 0f, 4f);

        // Force airborne state
        _forwardDir = Vector3.ProjectOnPlane(_forwardDir, Vector3.up).normalized;
            
        _isGrounded = false;
        _lastGroundedTime = -999f;
        _jumpedThisFrame = true;
       // _suppressHorizontalThisFrame = true;
    }

    void GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, _groundCheckDistance,LayerMask.GetMask("Terrain")))
        {
            _isGrounded = true;
            _groundNormal = hit.normal;
            _lastGroundedTime = Time.time;
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > maxRideableSlope)
            {
                _isGrounded = false;
                return;
            }
// Landing detection
            if (!_wasGrounded && _verticalVelocity < 0f && 
                Time.time - _lastLandingTime > _landingCooldown)
            {
                _lastLandingTime = Time.time;
                float align = Vector3.Dot(hit.normal, Vector3.up);
                _speed += _landingBoost * align;
                _verticalVelocity = 0f;
            }
        }
        else
        {
            _isGrounded = false;
        }
    }
    void GroundMovement()
    {
        // there needs to be a way to get off the slope without jumping
// Project forward onto slope
        _forwardDir = Vector3.ProjectOnPlane(_forwardDir, _groundNormal).normalized;


// Turning (rotate direction, NOT velocity)
        float speedFactor = 1f + (_speed * _speedTurnScale);
        float turn = _turnRate * _smoothInput * speedFactor;
        _forwardDir = Quaternion.AngleAxis(turn * Time.deltaTime, _groundNormal) * _forwardDir;
//down hill bias
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, _groundNormal).normalized;

        float steepness = Mathf.InverseLerp(30f, maxRideableSlope, slopeAngle);

        _forwardDir = Vector3.Slerp(_forwardDir, downhill, steepness * _downhillBias);
        
        board.transform.rotation = Quaternion.LookRotation(_forwardDir);
        
//  acceleration
        float slopeFactor = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
        float downhillDot = Vector3.Dot(_forwardDir, downhill);
        float angleToDownhill = Vector3.Angle(_forwardDir, downhill);
        
        float accel;
        if (!_jumpedThisFrame)
        {
            if (downhillDot > 0f)
            {
                accel = downhillDot * slopeFactor * _downhillAccel;
            }
            else
            {
                float uphillPenalty = (slopeAngle > minSpeedSlopeLimit) ? 3.0f : 2.5f;
                accel = downhillDot * slopeFactor * _downhillAccel * uphillPenalty;
            }
            _speed += accel * Time.deltaTime;
        }
        _speed -= _friction * Time.deltaTime;
        
        if (_isGrounded &&
            _speed < 0.1f &&
            slopeAngle > minSpeedSlopeLimit &&
            angleToDownhill > 90f &&
            downhillDot <= 0f)
        {
            
            _forwardDir = Vector3.ProjectOnPlane(-_forwardDir, _groundNormal).normalized;
            
            _speed = 4f;
        }
        if (slopeAngle <= minSpeedSlopeLimit)
        {
            _speed = Mathf.Clamp(_speed, _minSpeed, _maxSpeed);
        }
        else if (slopeAngle <= maxRideableSlope)
        {
            _speed = Mathf.Clamp(_speed, 0f, _maxSpeed);
        }
        else
        {
            _speed = Mathf.Max(_speed, 0f);
        }


// Stick to ground
        bool goingUphill = Vector3.Dot(_forwardDir, Vector3.down) < 0f;
        bool stopped = _speed < 0.1f;

        Vector3 stick = Vector3.zero;

        if (!stopped && !goingUphill && _verticalVelocity <= 0f)
        {
            stick = -_groundNormal * _groundStickForce;
        }
        
        velocity = _forwardDir * _speed + stick;
        
        
        
        _controller.Move(velocity * Time.deltaTime);


// Align model to slope
        Quaternion slopeRot = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, slopeRot, 12f * Time.deltaTime);
    }
    void AirMovement()
    {
// Air turning
        _forwardDir = Quaternion.AngleAxis(
            _airTurnRate * _smoothInput * Time.deltaTime,
            Vector3.up
        ) * _forwardDir;
        board.transform.rotation = Quaternion.LookRotation(_forwardDir);

// Gravity
        _verticalVelocity += _gravity * _airGravityScale * Time.deltaTime;


        Vector3 horizontal = _suppressHorizontalThisFrame ? Vector3.zero : _forwardDir * _speed;

        Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);


// Upright stabilize
        Quaternion upright = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, upright, 4f * Time.deltaTime);
    }

    private void HandleInput()
    {
        _smoothInput = Mathf.SmoothDamp(_smoothInput, _moveDirection.x, ref _smoothInputVel, _inputSmoothTime);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + _forwardDir * 3f);


        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + _groundNormal * 2f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + velocity * 2f);
    }

    public void StopMovement()
    {
        _speed = 0;
        _isPaused = true;
        StartCoroutine(Pause());
    }
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(0.1f);
        _isPaused = false;
    }
    
}
