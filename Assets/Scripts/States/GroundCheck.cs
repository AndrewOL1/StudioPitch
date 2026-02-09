using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float maxRideableSlope;
    public bool IsGrounded;
    private RaycastHit hit;
    public Vector3 GroundNormal;
    private float _lastGroundedTime;
    RaycastHit _hitInfo;
    
    public void GroundCheckUpdate()
    {
        RaycastHit hit;
        
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        bool hitGround = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance,LayerMask.GetMask("Terrain"));
        if (!hitGround)
        {
            IsGrounded = false;
            return;
        }
        

        _hitInfo = hit;
        GroundNormal = hit.normal;
        
        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxRideableSlope)
        {
            IsGrounded = false;
            return;
        }
            
        IsGrounded = true;
        _lastGroundedTime = Time.time;
        
// Landing detection
/*
            if (!_wasGrounded && _verticalVelocity < 0f &&
                Time.time - _lastLandingTime > _landingCooldown)
            {
                _lastLandingTime = Time.time;
                float align = Vector3.Dot(hit.normal, Vector3.up);
                speed += landingBoost * align;
                _verticalVelocity = 0f;
            }
            */
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // ===== DOWNWARD GROUND CHECK =====
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        Gizmos.DrawLine(
            origin,
            origin + Vector3.down * groundCheckDistance
        );

        // Draw hit point
        if (IsGrounded)
        {
            Gizmos.DrawSphere(_hitInfo.point, 0.1f);
        }
    }
}
