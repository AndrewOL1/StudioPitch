using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using PurrNet.StateMachine;

namespace States
{
    public class CrashState : StateNode
    {
        private PlayerInput _input;
        [SerializeField] private float recoveryTime;
        private bool _fell;
        [SerializeField] private RaceState raceState;
        [SerializeField] private GroundCheck groundCheck;
        public override void Enter(bool asServer)
        {
            Debug.Log("Entered CrashedState");
            //_input = GetComponent<PlayerInput>();
            _fell = true;
            StartCoroutine(FallTimer(asServer));
        }

        public override void StateUpdate(bool asServer)
        {
            //for a set time be stunned
        }

        IEnumerator FallTimer(bool asServer)
        {
            yield return new WaitForSeconds(recoveryTime);
            machine.SetState(raceState);
        }

        public override void Exit(bool asServer)
        {
            Debug.Log("Exited CrashState");
        }
    }
}
