using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineRaceTracker : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] private Spline _raceSpline;//SF for testing
    [SerializeField] RaceProgressBar progressBar;
    private float[] _cumulativeDistances;
    private List<Vector3> _splinePoints = new List<Vector3>();
    private float _totalSplineLength;

    void Start()
    {
        _raceSpline = splineContainer.Spline;
        CalculateSplineData();
    }

    private void CalculateSplineData()
    {
        _cumulativeDistances = new float[_raceSpline.Count];
        _cumulativeDistances[0] = 0f;
        _totalSplineLength = 0f;
        
        for (int i = 0; i < _raceSpline.Count-1; i++)
        {
            _splinePoints.Add(new Vector3(_raceSpline[i].Position.x + transform.position.x, _raceSpline[i].Position.y + transform.position.y , _raceSpline[i].Position.z + transform.position.z));
        }

        for (int i = 1; i < _raceSpline.Count-1; i++)
        {
            float segmentLength = Vector3.Distance(_splinePoints[i-1], _splinePoints[i]);
            _totalSplineLength += segmentLength;
            _cumulativeDistances[i] = _totalSplineLength;
        }
    }

    public (float progress, float distance) GetPlayerProgress(Vector3 playerPosition)
    {
        if (_splinePoints.Count < 2)
        {
            Debug.LogError("Need at least 2 spline points!");
            return (0f, 0f);
        }
        int closestSegment = FindClosestSegment(playerPosition);
        
        float distanceAlongSpline = GetExactDistanceAlongSpline(playerPosition, closestSegment);
        
        float progress = distanceAlongSpline / _totalSplineLength;
        
        progressBar.UpdateProgress(progress); //this is just for testing and will only work in singleplayer 
        
        return (progress, distanceAlongSpline);
    }

    private float GetExactDistanceAlongSpline(Vector3 playerPosition, int closestSegment)
    {
        if (closestSegment < 0 || closestSegment >= _splinePoints.Count - 1)
        {
            return 0f;
        }
        
        float distanceToSegmentStart = _cumulativeDistances[closestSegment];
        
        Vector3 segmentStart = _splinePoints[closestSegment];
        Vector3 segmentEnd = _splinePoints[closestSegment + 1];
        
        Vector3 projection = GetClosestPointOnSegment(playerPosition, segmentStart, segmentEnd);
        float distanceInSegment = Vector3.Distance(segmentStart, projection);
        
        return distanceToSegmentStart + distanceInSegment;
    }

    private int FindClosestSegment(Vector3 playerPosition)
    {
        if (_splinePoints.Count < 2) return 0;
        
        float minDistance = float.MaxValue;
        int closestSegment = 0;
        
        for (int i = 0; i < _splinePoints.Count - 1; i++) // Stop at Count - 2
        {
            Vector3 closestPoint = GetClosestPointOnSegment(
                playerPosition, _splinePoints[i], _splinePoints[i + 1]);
            
            float distance = Vector3.Distance(playerPosition, closestPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSegment = i;
            }
        }
        
        return closestSegment;
    }

    private Vector3 GetClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        return a + Mathf.Clamp01(t) * ab;
    }
}
