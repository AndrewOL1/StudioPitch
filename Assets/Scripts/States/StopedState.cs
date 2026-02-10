using PurrNet.StateMachine;
using States;
using UnityEngine;
using UnityEngine.InputSystem;

public class StopedState : StateNode
{
    CharacterController _controller;
    [SerializeField] private RaceState raceState;
    
    public override void Enter(bool asServer)
    {
        Debug.Log("Entered StopedState");
        _controller = GetComponent<CharacterController>();
        _controller.Move(Vector3.zero);
    }

    public override void StateUpdate(bool asServer)
    {
        
    }
        

    public override void Exit(bool asServer)
    {
        Debug.Log("Exited StopedState");
    }

    public void StartRace()
    {
        machine.SetState(raceState);
    }
}
