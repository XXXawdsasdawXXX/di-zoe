using System;
using System.Collections.Generic;
using System.Linq;
using Code.Game.AppTracker;
using UnityEngine;

namespace Code.UI.Windows.AppTime
{
    public class AppTimeController : UIPresenter<AppTimeListView>
    {
        [SerializeField] private WindowTracker   _tracker;

        private AppTimeData _savedData;
        private int _currentDays = 1;

        private void Start()
        {
            _savedData = AppTimeStorage.Load();
            view.OnPeriodChanged += OnPeriodChanged;
            Refresh(_currentDays);
        }

        private void OnDestroy()
        {
            view.OnPeriodChanged -= OnPeriodChanged;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveCurrentSession();
        }

        private void OnApplicationQuit()
        {
            SaveCurrentSession();
        }

        private void SaveCurrentSession()
        {
            AppTimeStorage.Save(_tracker.AppTime);
            _savedData = AppTimeStorage.Load();
            Refresh(_currentDays);
        }

        private void OnPeriodChanged(int days)
        {
            _currentDays = days;
            Refresh(days);
        }

        private void Refresh(int days)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // Все приложения за период (включая сегодня)
            List<AppEntry> periodEntries = _savedData.Entries
                .Where(e => IsWithinDays(e.Date, days))
                .ToList();

            // Группируем по приложению — суммируем время за каждый день отдельно
            // чтобы считать среднее как среднее по дням, а не сумму
            Dictionary<string, Dictionary<string, float>> appDays = periodEntries
                .GroupBy(e => e.AppName)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(e => e.Date)
                          .ToDictionary(d => d.Key, d => d.Sum(e => e.TotalSeconds) / 60f));

            // Добавляем текущую сессию к сегодняшним данным
            foreach ((string app, float seconds) in _tracker.AppTime)
            {
                if (!appDays.ContainsKey(app))
                    appDays[app] = new Dictionary<string, float>();

                if (!appDays[app].ContainsKey(today))
                    appDays[app][today] = 0f;

                appDays[app][today] += seconds / 60f;
            }

            // Строим UI entries
            // today = сегодняшнее время, avg = среднее по остальным дням периода
            AppTimeEntryUI[] uiEntries = appDays
                .Select(kv =>
                {
                    float todayMins = kv.Value.TryGetValue(today, out float t) ? t : 0f;

                    List<float> otherDays = kv.Value
                        .Where(d => d.Key != today)
                        .Select(d => d.Value)
                        .ToList();

                    float avgMins = otherDays.Count > 0
                        ? otherDays.Average()
                        : 0f;

                    return new AppTimeEntryUI
                    {
                        AppName      = kv.Key,
                        TodayMinutes = todayMins,
                        AvgMinutes   = avgMins
                    };
                })
                .Where(e => e.TodayMinutes > 0 || e.AvgMinutes > 0)
                .OrderByDescending(e => e.TodayMinutes)
                .ToArray();

            float maxMinutes = uiEntries.Length > 0
                ? uiEntries.Max(e => Mathf.Max(e.TodayMinutes, e.AvgMinutes))
                : 1f;

            int increased = uiEntries.Count(e => e.TodayMinutes > e.AvgMinutes);
            int decreased = uiEntries.Count(e => e.TodayMinutes < e.AvgMinutes);
            float total   = uiEntries.Sum(e => e.TodayMinutes);

            view.SetSummary(total, increased, decreased, days);
            view.SetRows(uiEntries, maxMinutes);
        }

        private static bool IsWithinDays(string dateStr, int days)
        {
            if (!DateTime.TryParse(dateStr, out DateTime date)) return false;
            return (DateTime.Now.Date - date.Date).TotalDays < days;
        }
    }
}