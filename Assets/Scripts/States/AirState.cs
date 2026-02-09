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
            throw new System.NotImplementedException();
        }

        private void AirMovement()
        {
            throw new System.NotImplementedException();
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Entered state");
            _moveAction.performed -= OnMovePrefromed;
        }
    }
}
