using System;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace States
{
    public class RaceState : StateNode
    {
        # region variables
        private PlayerInput _input;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private Vector2 _moveDirection;
        private bool _inputJump;
        
        
        [Header("Camera Control")]
        [SerializeField] private CameraController cameraController;
        [SerializeField] private bool isCameraBehind = true; // true = camera behind player

        public event Action<bool> OnDirectionFlipped;
    
    
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 45f;
    [SerializeField] private float downhillAccel = 25f;
    [SerializeField] private float friction = 1f;
    [SerializeField] float maxRideableSlope = 55f;
    [SerializeField] float minSpeedSlopeLimit = 10f;
    
    [Header("Turning")]
    [SerializeField]private float turnRate = 120f; // degrees per second
    [SerializeField]private float speedTurnScale = 0.015f; // higher = wider turns at speed
    [SerializeField]private float downhillBias = 15f;
    
    [Header("Gravity")]
    [SerializeField]private float gravity = -25f;
    
    [Header("Jump")]
    [SerializeField]private float jumpForce = 10f;
    [SerializeField]private float jumpGraceTime = 0.15f;
    
    [Header("Grounding")]
    [SerializeField]private float groundCheckDistance = 1.5f;
    
    private float _groundStickForce = 2f,_lastGroundedTime;
    
    [Header("Landing Boost")]
    [SerializeField]private float landingBoost = 6f;
    
    [Header("Smoothing")]
    [SerializeField]private float inputSmoothTime = 0.1f;
    
    [Header("States")]
    [SerializeField] private AirState airState;
    [SerializeField] private RaceFinishedState raceFinishedState;
    [SerializeField] private CrashState crashState;
    [SerializeField] private GroundCheck groundCheck;
    
    Quaternion _airRotation;
    bool _capturedAirRotation;
    
    [SerializeField]private RaycastHit _hitInfo;
    
    public float speed;
    public Vector3 velocity;
    float _verticalVelocity;
    
    Vector3 _forwardDir;
    Vector3 _groundNormal;
    
    [SerializeField]bool _isGrounded;
    
    float _smoothInput;
    float _smoothInputVel;
    
    CharacterController _controller;
    
    private bool _jumpedThisFrame,_suppressHorizontalThisFrame;
    
    float _lastLandingTime;
    float _landingCooldown = 0.1f;

    
    
    //testing
    private SplineRaceTracker _splineRaceTracker;
        # endregion
        public override void Enter(bool asServer)
        {
            Debug.Log("Entered RaceState");
            _input = GetComponent<PlayerInput>();
            _moveAction = _input.actions.FindAction("Move");
            _moveAction.performed += OnMovePrefromed;
            _moveAction.canceled += OnMovePrefromed;
            _jumpAction = _input.actions.FindAction("Jump");
            _jumpAction.performed += OnJumpPreformed;
            _jumpAction.canceled += OnJumpPreformed;
            _controller = GetComponent<CharacterController>();
            _forwardDir = transform.forward;
        }

        #region InputHandlers
        
        private void OnJumpPreformed(InputAction.CallbackContext context)
        {
            _inputJump = context.ReadValueAsButton();
        }
        private void OnMovePrefromed(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>().normalized;
        }
        # endregion

        public override void StateUpdate(bool asServer)
        {
            HandleInput();
            groundCheck.GroundCheckUpdate();
            if(!groundCheck.IsGrounded)
                machine.SetState(airState,false);
            HandleJump();
            GroundMovement();
        }
        
        private void HandleInput()
        {
            _smoothInput = Mathf.SmoothDamp(_smoothInput, _moveDirection.x, ref _smoothInputVel, inputSmoothTime);
        }

        private void GroundMovement()
        {
            // get ground check varibles
            _groundNormal = groundCheck.GroundNormal;
            _isGrounded = groundCheck.IsGrounded;
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
        
        //board.transform.rotation = Quaternion.LookRotation(_forwardDir);
        
//  acceleration
        float slopeFactor = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
        float downhillDot = Vector3.Dot(_forwardDir, downhill);
        float angleToDownhill = Vector3.Angle(_forwardDir, downhill);
        
        float accel;
        
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
        
        speed -= friction * Time.deltaTime;
        
        if (_isGrounded && speed < 0.1f && slopeAngle > minSpeedSlopeLimit && angleToDownhill > 90f && downhillDot <= 0f)
        {
            _forwardDir = Vector3.ProjectOnPlane(-_forwardDir, _groundNormal).normalized;
            speed = 4f;
            
            isCameraBehind = !isCameraBehind;
            
            OnDirectionFlipped?.Invoke(isCameraBehind);

            if (cameraController)
            {
                cameraController.SwitchPerspective(isCameraBehind);
            }
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
        transform.rotation = Quaternion.LookRotation(_forwardDir);
        }

        private void HandleJump()
        {
            if (!_inputJump) return;
            //_controller.Move(Vector3.up * 0.05f);
            
            // Force airborne state
            _forwardDir = Vector3.ProjectOnPlane(_forwardDir, Vector3.up).normalized;
            _inputJump=false;
            machine.SetState(airState,true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Jump"))
                machine.SetState(airState,false);
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Exited RaceState");
            _moveAction.performed -= OnMovePrefromed;
            _jumpAction.performed -= OnJumpPreformed;
            _jumpAction.canceled -= OnJumpPreformed;
            _moveAction.canceled -= OnMovePrefromed;
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 origin = transform.position + Vector3.up * 0.5f;

            // ===== DOWNWARD GROUND CHECK =====
            Gizmos.color =  Color.yellow;

            Gizmos.DrawLine(
                origin,
                origin + _forwardDir * _forwardDir.magnitude
            );
        }
    }
}
