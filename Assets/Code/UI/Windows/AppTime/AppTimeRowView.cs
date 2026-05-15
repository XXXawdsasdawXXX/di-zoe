using Code.Core.GameLoop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Windows.AppTime
{
    public class AppTimeRowView : MonoBehaviour, IPoolEntity
    {
        [SerializeField] private TMP_Text _appNameText;
        [SerializeField] private TMP_Text _todayTimeText;
        [SerializeField] private TMP_Text _avgTimeText;
        [SerializeField] private TMP_Text _diffText;

        [SerializeField] private RectTransform _baseBar;
        [SerializeField] private RectTransform _diffBar;
        [SerializeField] private RectTransform _marker;
        [SerializeField] private Image _diffBarImage;

        [SerializeField] private int _index;

        [Header("Colors")]
        [SerializeField] private Color _positiveColor = new Color(0.39f, 0.43f, 0.07f);
        [SerializeField] private Color _negativeColor = new Color(0.60f, 0.24f, 0.11f);

        private float _barMaxWidth;

        public int Index => _index;

        public bool IsEnabled() => gameObject.activeSelf;
        public void Enable()    => gameObject.SetActive(true);
        public void Disable()   => gameObject.SetActive(false);

        private void Awake()
        {
            _barMaxWidth = ((RectTransform)_baseBar.parent).rect.width;
        }

        public void Setup(string appName, float todayMinutes, float avgMinutes, float maxMinutes)
        {
            _appNameText.text   = appName;
            _todayTimeText.text = FormatTime(todayMinutes);
            _avgTimeText.text   = $"avg: {FormatTime(avgMinutes)}";

            float diff  = todayMinutes - avgMinutes;
            bool  isUp  = diff >= 0;
            float pct   = avgMinutes > 0 ? (diff / avgMinutes) * 100f : 0f;
            string sign = isUp ? "+" : "";

            _diffText.text      = $"{sign}{Mathf.RoundToInt(pct)}% ({sign}{FormatTime(Mathf.Abs(diff))})";
            _diffText.color     = isUp ? _positiveColor : _negativeColor;
            _diffBarImage.color = isUp ? _positiveColor : _negativeColor;

            float baseW = _barMaxWidth * (Mathf.Min(todayMinutes, avgMinutes) / maxMinutes);
            float diffW = _barMaxWidth * (Mathf.Abs(diff) / maxMinutes);

            _baseBar.sizeDelta = new Vector2(baseW, _baseBar.sizeDelta.y);
            _diffBar.sizeDelta = new Vector2(diffW, _diffBar.sizeDelta.y);

            float markerX = _barMaxWidth * (Mathf.Min(todayMinutes, avgMinutes) / maxMinutes);
            _marker.anchoredPosition = new Vector2(markerX, _marker.anchoredPosition.y);
        }

        private static string FormatTime(float minutes)
        {
            int m = Mathf.RoundToInt(Mathf.Abs(minutes));
            if (m < 60) return $"{m} min";
            return $"{m / 60}h {m % 60}m";
        }
    }
}