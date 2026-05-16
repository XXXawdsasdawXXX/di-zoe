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
        [SerializeField] private float _barMaxWidth = 340f;
        
        [Header("Colors")]
        [SerializeField] private Color _positiveColor = new Color(0.39f, 0.43f, 0.07f);
        [SerializeField] private Color _negativeColor = new Color(0.60f, 0.24f, 0.11f);

        public int Index => _index;

        public bool IsEnabled() => gameObject.activeSelf;
        public void Enable()    => gameObject.SetActive(true);
        public void Disable()   => gameObject.SetActive(false);
        
        public void Setup(string appName, float todayMinutes, float avgMinutes, float maxMinutes)
        {
            _appNameText.text   = appName;
            _todayTimeText.text = FormatTime(todayMinutes);
            _avgTimeText.text   = $"avg: {FormatTime(avgMinutes)}";

            float diff  = todayMinutes - avgMinutes;
            bool  isUp  = diff >= 0;
    

            // Засечка стоит на позиции меньшего значения
            float baseVal = Mathf.Min(todayMinutes, avgMinutes);
            float markerX = maxMinutes > 0 ? _barMaxWidth * (baseVal / maxMinutes) : 0f;
            // -_barMaxWidth*0.5 потому что anchor у marker = 0,0.5 от левого края контейнера

            _marker.anchoredPosition = new Vector2(markerX, 0f);

            // Цветная полоска начинается от засечки и идёт вправо
            float diffW = _barMaxWidth * (Mathf.Abs(diff) / maxMinutes);
            _diffBar.anchoredPosition = new Vector2(markerX, 0f);
            _diffBar.sizeDelta        = new Vector2(diffW, _diffBar.sizeDelta.y);

            _diffBarImage.color = isUp ? _positiveColor : _negativeColor;

            // Текст дифа
            float pct     = avgMinutes > 0 ? (diff / avgMinutes) * 100f : 0f;
            string sign   = isUp ? "+" : "";
            _diffText.text  = $"{sign}{Mathf.RoundToInt(pct)}% ({sign}{FormatTime(Mathf.Abs(diff))})";
            _diffText.color = isUp ? _positiveColor : _negativeColor;
        }

        private static string FormatTime(float minutes)
        {
            int m = Mathf.RoundToInt(Mathf.Abs(minutes));
            if (m < 60) return $"{m} min";
            return $"{m / 60}h {m % 60}m";
        }
    }
}