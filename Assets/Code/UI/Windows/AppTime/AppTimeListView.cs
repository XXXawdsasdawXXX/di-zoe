using Code.Core.GameLoop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppTracker.UI
{
    public class AppTimeListView : MonoBehaviour
    {
        [Header("Summary")]
        [SerializeField] private TMP_Text _totalTodayText;
        [SerializeField] private TMP_Text _increasedCountText;
        [SerializeField] private TMP_Text _decreasedCountText;
        [SerializeField] private TMP_Text _periodText;

        [Header("Input")]
        [SerializeField] private TMP_InputField _daysInput;
        [SerializeField] private Button _showButton;

        [Header("Pool")]
        [SerializeField] private MonoPool<AppTimeRowView> _rowPool;
        [SerializeField] private ScrollRect _scrollRect;

        public event System.Action<int> OnPeriodChanged;

        private void Awake()
        {
            _showButton.onClick.AddListener(() =>
            {
                if (int.TryParse(_daysInput.text, out int days) && days > 0)
                    OnPeriodChanged?.Invoke(days);
            });

            _daysInput.onSubmit.AddListener(val =>
            {
                if (int.TryParse(val, out int days) && days > 0)
                    OnPeriodChanged?.Invoke(days);
            });
        }

        public void SetSummary(float totalMinutes, int increased, int decreased, int days)
        {
            _totalTodayText.text     = FormatTime(totalMinutes);
            _increasedCountText.text = $"↑ {increased}";
            _decreasedCountText.text = $"↓ {decreased}";
            _periodText.text         = $"{days} days";
        }

        public void SetRows(AppTimeEntryUI[] entries, float maxMinutes)
        {
            _rowPool.DisableAll();

            foreach (AppTimeEntryUI entry in entries)
            {
                AppTimeRowView row = _rowPool.GetNext();
                row.Setup(entry.AppName, entry.TodayMinutes, entry.AvgMinutes, maxMinutes);
            }

            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private static string FormatTime(float minutes)
        {
            int m = Mathf.RoundToInt(minutes);
            if (m < 60) return $"{m} min";
            return $"{m / 60}h {m % 60}m";
        }
    }

    public struct AppTimeEntryUI
    {
        public string AppName;
        public float  TodayMinutes;
        public float  AvgMinutes;
    }
}