using PurrNet;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class SsxPlayerController : MonoBehaviour
{
    # region variables
    [Header("References")]
    public Transform cameraTransform;
    
    
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 45f;
    [SerializeField] private float downhillAccel = 25f;
    [SerializeField] private float friction = 1f;
    [SerializeField] float maxRideableSlope = 55f;
    [SerializeField] float minSpeedSlopeLimit = 10f;
    
    [Header("Turning")]
    [SerializeField]private float turnRate = 120f; // degrees per second
    [SerializeField]private float airTurnRate = 35f;
    [SerializeField]private float speedTurnScale = 0.015f; // higher = wider turns at speed
    [SerializeField]private float downhillBias = 15f;
    
    [Header("Gravity")]
    [SerializeField]private float gravity = -25f;
    [SerializeField]private float airGravityScale = 0.6f;
    
    [Header("Jump")]
    [SerializeField]private float jumpForce = 10f;
    [SerializeField]private float jumpGraceTime = 0.15f;
    
    [Header("Grounding")]
    [SerializeField]private float groundCheckDistance = 1.5f;
    [SerializeField]private float groundCheckForward = 1.2f;
    [SerializeField]private float groundCheckTolerance = 0.6f;
    private float _groundStickForce = 2f,_lastGroundedTime;
    
    [Header("Landing Boost")]
    [SerializeField]private float landingBoost = 6f;
    
    [Header("Smoothing")]
    [SerializeField]private float inputSmoothTime = 0.1f;

    [Header("Editor")] [SerializeField] private GameObject board;
    
    Quaternion _airRotation;
    bool _capturedAirRotation;
    
    [SerializeField]private RaycastHit _hitInfo;
    
    public float speed;
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
    
    //testing
    private SplineRaceTracker _splineRaceTracker;

    #endregion

    #region InputHandlers

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
        speed = minSpeed;
        _forwardDir = transform.forward;
        _splineRaceTracker = FindObjectOfType<SplineRaceTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_isPaused)return;
        HandleInput();
        GroundCheck();
        HandleJump();

        if (_isGrounded)
        {
            _capturedAirRotation = false;
            GroundMovement();
        }
        else
        {
            if (!_capturedAirRotation)
            {
                _airRotation = transform.rotation;
                _capturedAirRotation = true;
            }
            AirMovement();
        }

        _wasGrounded = _isGrounded;
        _jumpedThisFrame = false;
        _suppressHorizontalThisFrame = false;
    }

    private void FixedUpdate()
    {
        //testing 
        //UpdateProgress();
    }

    private void HandleJump()
    {
        if (!_inputJump || !(Time.time - _lastGroundedTime <= jumpGraceTime)) return;
        //_controller.Move(Vector3.up * 0.05f);
        _verticalVelocity = jumpForce + Mathf.Clamp(speed * 0.05f, 0f, 4f);

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
        
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        bool hitGround = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance,LayerMask.GetMask("Terrain"));
        if (!hitGround)
        {
            _isGrounded = false;
            return;
        }
        

        _hitInfo = hit;
        _groundNormal = hit.normal;
        
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxRideableSlope)
        {
            _isGrounded = false;
            return;
        }
        
        RaycastHit forwardHit;

        Vector3 probeOrigin =
            origin +
            _forwardDir.normalized * groundCheckForward;

        bool forwardGround = Physics.Raycast(
            probeOrigin,
            Vector3.down,
            out forwardHit,
            groundCheckDistance,
            LayerMask.GetMask("Terrain"));

        if (!forwardGround)
        {
            _isGrounded = false;
            return;
        }
        float drop = hit.point.y - forwardHit.point.y;

        if (drop > groundCheckTolerance)
        {
            _isGrounded = false;
            return;
        }
        _isGrounded = true;
        _lastGroundedTime = Time.time;
        
// Landing detection
        if (!_wasGrounded && _verticalVelocity < 0f && 
            Time.time - _lastLandingTime > _landingCooldown)
        {
            _lastLandingTime = Time.time;
            float align = Vector3.Dot(hit.normal, Vector3.up);
            speed += landingBoost * align;
            _verticalVelocity = 0f;
        }
    }
    void GroundMovement()
    {
// Project forward onto slope
        _forwardDir = Vector3.ProjectOnPlane(_forwardDir, _groundNormal).normalized;


// Turning (rotate direction)
        float speedFactor = 1f + (speed * speedTurnScale);
        float turn = turnRate * _smoothInput * speedFactor;
        _forwardDir = Quaternion.AngleAxis(turn * Time.deltaTime, _groundNormal) * _forwardDir;
//down hill bias
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, _groundNormal).normalized;

        float steepness = Mathf.InverseLerp(30f, maxRideableSlope, slopeAngle);

        _forwardDir = Vector3.Slerp(_forwardDir, downhill, steepness * downhillBias);
        
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
                accel = downhillDot * slopeFactor * downhillAccel;
            }
            else
            {
                float uphillPenalty = (slopeAngle > minSpeedSlopeLimit) ? 3.0f : 2.5f;
                accel = downhillDot * slopeFactor * downhillAccel * uphillPenalty;
            }
            speed += accel * Time.deltaTime;
        }
        speed -= friction * Time.deltaTime;
        
        if (_isGrounded &&
            speed < 0.1f &&
            slopeAngle > minSpeedSlopeLimit &&
            angleToDownhill > 90f &&
            downhillDot <= 0f)
        {
            
            _forwardDir = Vector3.ProjectOnPlane(-_forwardDir, _groundNormal).normalized;
            
            speed = 4f;
        }
        if (slopeAngle <= minSpeedSlopeLimit)
        {
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        }
        else if (slopeAngle <= maxRideableSlope)
        {
            speed = Mathf.Clamp(speed, 0f, maxSpeed);
        }
        else
        {
            speed = Mathf.Max(speed, 0f);
        }


// Stick to ground
        bool goingUphill = Vector3.Dot(_forwardDir, Vector3.down) < 0f;
        bool stopped = speed < 0.1f;

        Vector3 stick = Vector3.zero;

        if (!stopped && !goingUphill && _verticalVelocity <= 0f)
        {
            stick = -_groundNormal * _groundStickForce;
        }
        
        velocity = _forwardDir * speed + stick;
        
        
        
        _controller.Move(velocity * Time.deltaTime);


// Align model to slope
        Quaternion slopeRot = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, slopeRot, 12f * Time.deltaTime);
    }
    void AirMovement()
    {
// Air turning
        _forwardDir = Quaternion.AngleAxis(
            airTurnRate * _smoothInput * Time.deltaTime,
            Vector3.up
        ) * _forwardDir;
        //board.transform.rotation = Quaternion.LookRotation(_forwardDir);
        Quaternion boardRot = Quaternion.LookRotation(_forwardDir, _groundNormal);

        board.transform.rotation = boardRot;
// Gravity
        _verticalVelocity += gravity * airGravityScale * Time.deltaTime;
        
        Vector3 horizontal = _suppressHorizontalThisFrame ? Vector3.zero : _forwardDir * speed;

        Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
        _controller.Move(velocity * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _airRotation,
            2f * Time.deltaTime);
        board.transform.rotation = transform.rotation;
    }

    private void HandleInput()
    {
        _smoothInput = Mathf.SmoothDamp(_smoothInput, _moveDirection.x, ref _smoothInputVel, inputSmoothTime);
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // ===== DOWNWARD GROUND CHECK =====
        Gizmos.color = _isGrounded ? Color.green : Color.red;

        Gizmos.DrawLine(
            origin,
            origin + Vector3.down * groundCheckDistance
        );

        // Draw hit point
        if (_isGrounded)
        {
            Gizmos.DrawSphere(_hitInfo.point, 0.1f);
        }

        // ===== FORWARD PROBE CHECK =====
        Vector3 probeOrigin =
            origin +
            _forwardDir.normalized * groundCheckForward;

        Gizmos.color = Color.cyan;

        // Draw forward offset line
        Gizmos.DrawLine(origin, probeOrigin);

        // Draw downward probe ray
        Gizmos.DrawLine(
            probeOrigin,
            probeOrigin + Vector3.down * groundCheckDistance
        );

        Gizmos.DrawSphere(probeOrigin, 0.07f);
    }

    public void StopMovement()
    {
        speed = 0;
        _isPaused = true;
        StartCoroutine(Pause());
    }
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(0.1f);
        _isPaused = false;
    }

    public float UpdateProgress() // call by game Manager
    {
        float progress,distance;
        (progress,distance)=_splineRaceTracker.GetPlayerProgress(transform.position);
        InstanceHandler.GetInstance<SplineRaceTracker>();
        return progress;
    }

    private void Awake()
    {
        //We register the SSXPlayerController instance
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        //Upon being destroyed, we unregister the game manager instance
        InstanceHandler.UnregisterInstance<GameManager>();
    }


}
