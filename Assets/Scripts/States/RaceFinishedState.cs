using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace States
{
    public class RaceFinishedState : StateNode
    {
        private PlayerInput _input;
        private InputAction jumpAction;
        public override void Enter(bool asServer)
        {
            Debug.Log("Entered state1");
            _input = GetComponent<PlayerInput>();
            _input.actions.FindAction("Idle");
        }

        public override void StateUpdate(bool asServer)
        {
            
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Entered state");
        }
    }
}