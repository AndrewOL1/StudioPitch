using System;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace States
{
    public class AirState : StateNode<Vector3>
    {
        PlayerInput _input;
        InputAction _moveAction;
        private Vector2 _moveDirection;
        [SerializeField] private CrashState crashState;
        [SerializeField] private RaceState raceState;
        
        [SerializeField]private float airTurnRate = 35f;
        [SerializeField]private float airGravityScale = 0.6f;
        public override void Enter(bool asServer)
        {
            Debug.Log("Entered AirState");
            _input = GetComponent<PlayerInput>();
            _moveAction = _input.actions.FindAction("Move");
            _moveAction.performed += OnMovePrefromed;
        }

        private void OnMovePrefromed(InputAction.CallbackContext context)
        {
            _moveDirection = context.ReadValue<Vector2>().normalized;
        }

        public override void StateUpdate(bool asServer)
        {
            AirMovement();
            Trick();
        }

        private void Trick()
        {
            //rotate with a or d
            //flip with w or s
        }

        private void AirMovement()
        {
            //gravity
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Entered state");
            _moveAction.performed -= OnMovePrefromed;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                //tell if board is facing ground
                //if yes race state
                //else crash state
                machine.SetState(raceState);
            }
        }
    }
}
