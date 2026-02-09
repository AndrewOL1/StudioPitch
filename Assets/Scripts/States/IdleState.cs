using System;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace States
{
    public class IdleState : StateNode
    {
        private PlayerInput _input;
        private InputAction jumpAction;
        CharacterController _controller;
        [SerializeField] private RaceState raceState;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private GroundCheck groundCheck;
        public override void Enter(bool asServer)
        {
            Debug.Log("Entered IdleState");
            _input = GetComponent<PlayerInput>();
            _input.actions.FindAction("Idle");
            _controller = GetComponent<CharacterController>();
        }

        public override void StateUpdate(bool asServer)
        {
            groundCheck.GroundCheckUpdate();
            if(groundCheck.IsGrounded)
                machine.SetState(raceState);
            _controller.Move(Vector3.up * (gravity * Time.deltaTime));
        }
        

        public override void Exit(bool asServer)
        {
            Debug.Log("Exited Idle");
        }
    }
}
