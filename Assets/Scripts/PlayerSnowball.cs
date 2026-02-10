using System.Collections;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSnowball : MonoBehaviour
{
    private PlayerInput _input;
    private InputAction _attackAction;
    [SerializeField] private NetworkIdentity snowball;
    private NetworkIdentity _spawnedSnowball;
    
    [SerializeField] private Vector3 offset;

    private bool _onCooldown = false;

    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _attackAction = _input.actions.FindAction("Attack");
        _attackAction.performed += OnAttackPrefromed;
        _attackAction.canceled += OnAttackPrefromed;
    }

    private void OnAttackPrefromed(InputAction.CallbackContext obj)
    {
        if (_onCooldown)return;
        
        _spawnedSnowball = Instantiate(snowball, transform.position+offset, Quaternion.identity);
        
        _onCooldown = true;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        _onCooldown = false;
    }
}
