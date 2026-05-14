using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AppTracker
{
    [Serializable]
    public class AppTimeData
    {
        public List<AppEntry> Entries = new();
    }

    [Serializable]
    public class AppEntry
    {
        public string AppName;
        public float  TotalSeconds;
        public string Date; // "2026-05-06"
    }

    public static class AppTimeStorage
    {
        private static string Path => 
            System.IO.Path.Combine(Application.persistentDataPath, "apptime.json");

        public static AppTimeData Load()
        {
            Debug.Log($"save window tracker -> {Path}");
            
            if (!File.Exists(Path))
                return new AppTimeData();

            string json = File.ReadAllText(Path);
            return JsonUtility.FromJson<AppTimeData>(json) ?? new AppTimeData();
        }

        public static void Save(Dictionary<string, float> appTime)
        {
            AppTimeData data  = Load();
            string today      = DateTime.Now.ToString("yyyy-MM-dd");

            foreach (var (app, seconds) in appTime)
            {
                // Ищем запись за сегодня
                var entry = data.Entries.Find(e => e.AppName == app && e.Date == today);

                if (entry == null)
                {
                    data.Entries.Add(new AppEntry
                    {
                        AppName      = app,
                        TotalSeconds = seconds,
                        Date         = today
                    });
                }
                else
                {
                    entry.TotalSeconds += seconds;
                }
            }

            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        }
    }
}