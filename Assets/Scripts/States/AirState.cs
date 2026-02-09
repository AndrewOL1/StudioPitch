using System;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace States
{
    public class AirState : StateNode<bool>
    {
        PlayerInput _input;
        InputAction _moveAction;
        private Vector2 _moveDirection;
        [SerializeField] private CrashState crashState;
        [SerializeField] private RaceState raceState;
        
        [SerializeField]private float airTurnRate = 35f;
        [SerializeField]private float gravity = 0.6f;
        [SerializeField]private float jumpForce = 25f;
        CharacterController _controller;
        [SerializeField] private GroundCheck groundCheck;
        private float _verticalVelocity;
        private float _smoothInput;
        private Vector3 _startingForwardDirection;
        [SerializeField] private float groundCheckDistance;
        
        Vector3 _forwardDir, _velocity;
        public override void Enter(bool jumped, bool asServer)
        {
            Debug.Log("Entered AirState");
            _input = GetComponent<PlayerInput>();
            _moveAction = _input.actions.FindAction("Move");
            _moveAction.performed += OnMovePrefromed;
            _moveAction.canceled += OnMovePrefromed;
            _controller = GetComponent<CharacterController>();
            _velocity = _controller.velocity;
            _forwardDir = transform.forward;
            if(jumped)
                _verticalVelocity = _velocity.y + jumpForce;
            _startingForwardDirection = _forwardDir;
            AirMovement();
        }

        private void OnMovePrefromed(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>().normalized;
        }

        public override void StateUpdate(bool asServer)
        {
            groundCheck.GroundCheckUpdate();
            if (groundCheck.IsGrounded)
            {
                LandingCheck();
            }
            AirMovement();
            Trick();
        }

        private void Trick()
        {
            //rotate with a or d
            Quaternion horQuaternion = Quaternion.AngleAxis(airTurnRate * _moveDirection.x * Time.deltaTime,transform.forward);
            //flip with w or s
            Quaternion vertQuaternion = Quaternion.AngleAxis(airTurnRate * _moveDirection.y * Time.deltaTime,transform.right);
            
            Quaternion forwardQuaternion = horQuaternion * vertQuaternion;
            
            transform.rotation = transform.rotation * forwardQuaternion;
        }

        private void AirMovement()
        {
            _verticalVelocity +=gravity * Time.deltaTime;
            _velocity = new Vector3(_velocity.x,_verticalVelocity , _velocity.z);
            //maybe some friction
            _controller.Move(_velocity * Time.deltaTime);
            
        }
        
        public override void Exit(bool asServer)
        {
            Debug.Log("Exited AirState");
            _moveAction.performed -= OnMovePrefromed; ;
            _moveAction.canceled -= OnMovePrefromed;
        }

        private void LandingCheck()
        {
            RaycastHit hit;
            
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 frontDirection = origin + _startingForwardDirection * 0.5f;
            Vector3 backDirection = origin - _startingForwardDirection;
            bool hitFrontGround = Physics.Raycast(frontDirection,_startingForwardDirection, out hit, groundCheckDistance,LayerMask.GetMask("Terrain"));
            bool hitBackGround = Physics.Raycast(backDirection,-_startingForwardDirection, out hit, groundCheckDistance,LayerMask.GetMask("Terrain"));
            if (hitFrontGround || hitBackGround)
            {
                
                //get boost
                machine.SetState(raceState);
            }
            else
            {
                machine.SetState(crashState);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            LandingCheck();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 origin = transform.position + Vector3.up * 0.5f;

            Gizmos.color = Color.green;
            
            Gizmos.DrawLine(
                origin,
                origin + _startingForwardDirection + Vector3.Cross(_startingForwardDirection , transform.position).normalized * groundCheckDistance
            );
            
            Gizmos.DrawLine(
                origin,
                origin - _startingForwardDirection + -_startingForwardDirection * groundCheckDistance
            );

        }
    }
}
