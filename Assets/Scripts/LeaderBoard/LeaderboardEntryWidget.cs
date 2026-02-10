using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeaderBoard
{
    public class LeaderboardEntryWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RawImage avatarImage;
        [SerializeField] private Slider pointSlider;
        public string playerName;
        public int points;
        
        void Start()
        {
            nameText.text = playerName;
            pointSlider.value = points;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
