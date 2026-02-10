using States;
using UnityEngine;
using Unity.Cinemachine;
public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera ForwardsVirtualCamera,backwardsVirtualCamera;
    [SerializeField] private RaceState raceState;
    [SerializeField] private float switchSmoothTime = 0.5f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 5f;
    [SerializeField] private float cameraAngle = 30f;
    
    private Vector3 _behindOffset;
    private Vector3 _frontOffset;
    private Vector3 _targetOffset;
    private Vector3 _currentOffset;
    private bool _isBehindPlayer = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Calculate offsets
        _behindOffset = new Vector3(0f, cameraHeight, -cameraDistance);
        _frontOffset = new Vector3(0f, cameraHeight, cameraDistance);
        
        _currentOffset = _behindOffset;
        _targetOffset = _behindOffset;
        
        // Subscribe to player's flip event if available
        
        
    }

    // Update is called once per frame
    public void SwitchPerspective(bool behindPlayer)
    {
        _isBehindPlayer = behindPlayer;
        _targetOffset = _isBehindPlayer ? _behindOffset : _frontOffset;
        
        // Optional: Adjust angle for front view
        if (!_isBehindPlayer)
        {
            // Slightly different angle for front view
            Quaternion rotation = Quaternion.Euler(cameraAngle * 0.8f, 180f, 0f);
            _frontOffset = rotation * Vector3.back * cameraDistance;
            _frontOffset.y = cameraHeight;
        }
    }
}
