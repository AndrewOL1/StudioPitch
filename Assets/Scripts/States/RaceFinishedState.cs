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
            Debug.Log("Entered RaceFinishedState");
            
        }

        public override void StateUpdate(bool asServer)
        {
            
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Exited RaceFinishedState");
        }
    }
}