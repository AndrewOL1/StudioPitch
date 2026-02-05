using UnityEngine;
using UnityEngine.UI;

public class RaceProgressBar : MonoBehaviour
{
    [SerializeField] private Slider raceSlider;
    [SerializeField] private SplineRaceTracker raceTracker;

    public void UpdateProgress(float progress)
    {
        raceSlider.value=progress;
    }
}
