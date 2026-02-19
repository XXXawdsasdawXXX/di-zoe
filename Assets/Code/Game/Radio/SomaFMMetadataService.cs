using System;
using System.Collections;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Game.Radio
{
    /// <summary>
    /// Fetches and displays metadata from SomaFM API:
    /// current track, previous tracks, album cover, channel logo, listener count.
    /// Attach to the same GameObject as RadioPlayer, or reference it manually.
    /// </summary>
    public class SomaFMMetadataService : MonoBehaviour, IService, 
        IInitializeListener, IExitListener, ISubscriber
    {
        [Serializable]
        public class Model
        {
            public ReactiveProperty<string> CurrentTrack   = new("");
            public ReactiveProperty<string> PreviousTrack  = new("");
            public ReactiveProperty<string> ListenerCount  = new("");
            public ReactiveProperty<Texture2D> ChannelLogo = new(null);
        }

        public Model State { get; } = new();
        
        
        [Header("Settings")]
        [Tooltip("Интервал обновления трека в секундах")]
        public float _trackUpdateInterval = 30f;

        [Tooltip("Интервал обновления числа слушателей в секундах")]
        public float _listenersUpdateInterval = 60f;
        
        private RadioConfiguration _radioConfiguration;
        
        private string _currentChannelId;
        private Coroutine _trackPollingCoroutine;
        private Coroutine _listenersPollingCoroutine;
        private RadioPlayer _radioPlayer;


        #region Life

        public UniTask GameInitialize()
        {
            _radioConfiguration = Container.Instance.GetConfig<RadioConfiguration>();
            _radioPlayer = Container.Instance.GetService<RadioPlayer>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _radioPlayer.Channel.SubscribeToValue(OnChannelChanged);
        }
        
        public void Unsubscribe()
        {
            _radioPlayer.Channel.UnsubscibeFromValue(OnChannelChanged);
        }
        
        public void GameExit()
        {
            if (_trackPollingCoroutine != null) StopCoroutine(_trackPollingCoroutine);
            if (_listenersPollingCoroutine != null) StopCoroutine(_listenersPollingCoroutine);
        }
        
        #endregion

        // ─────────────────────────────────────────────
        // Точка входа: вызывается при смене канала
        // ─────────────────────────────────────────────

        /// <summary>
        /// Запускает обновление метаданных для выбранного канала.
        /// Вызывайте из RadioPlayer.ChangeRadioStation(int index).
        /// </summary>
        private void OnChannelChanged(int dropdownIndex)
        {
            if (dropdownIndex < 0 || dropdownIndex >= _radioConfiguration.Channels.Length)
            {
                Debug.LogWarning("SomaFMMetadata: некорректный индекс канала");
                return;
            }

            Debug.Log("Soma fm meta data - on channel changed");
            
            _currentChannelId = _radioConfiguration.Channels[dropdownIndex].Id;

            // Перезапускаем опрос
            if (_trackPollingCoroutine != null) StopCoroutine(_trackPollingCoroutine);
            if (_listenersPollingCoroutine != null) StopCoroutine(_listenersPollingCoroutine);

            _trackPollingCoroutine = StartCoroutine(_pollTrackInfo());
            _listenersPollingCoroutine = StartCoroutine(_pollListenerCount());

            // Логотип канала грузим один раз при смене
            StartCoroutine(FetchChannelLogo());
        }
        
        // ─────────────────────────────────────────────
        // 1. Текущий трек
        // ─────────────────────────────────────────────

        /// <summary>
        /// Получает текущий трек через SomaFM songs API.
        /// Обновляет currentTrackText и запускает загрузку обложки.
        /// </summary>
        private IEnumerator FetchCurrentTrack()
        {
            string url = $"https://somafm.com/songs/{_currentChannelId}.json";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchCurrentTrack failed: {request.error}");
                yield break;
            }

            try
            {
                SongListResponse data = JsonUtility.FromJson<SongListResponse>(request.downloadHandler.text);

                if (data.songs == null || data.songs.Length == 0)
                    yield break;

                SongEntry latest = data.songs[0];
                string trackLine = $"{latest.artist} — {latest.title}";

                State.CurrentTrack.Value = trackLine;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FetchCurrentTrack parse error: {e.Message}");
            }
        }
        
        
        /// <summary>
        /// Получает список последних треков (до 5 штук).
        /// Обновляет previousTracksText.
        /// </summary>
        private IEnumerator FetchPreviousTracks(int count = 5)
        {
            string url = $"https://somafm.com/songs/{_currentChannelId}.json";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchPreviousTracks failed: {request.error}");
                yield break;
            }

            try
            {
                SongListResponse data = JsonUtility.FromJson<SongListResponse>(request.downloadHandler.text);

                if (data.songs == null || data.songs.Length <= 1)
                    yield break;

                int start = 1; // Пропускаем первый (текущий) трек
                int end = Mathf.Min(start + count, data.songs.Length);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("Недавно играло:");

                for (int i = start; i < end; i++)
                {
                    SongEntry s = data.songs[i];
                    sb.AppendLine($"  {i}. {s.artist} — {s.title}");
                }

                State.PreviousTrack.Value = sb.ToString();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FetchPreviousTracks parse error: {e.Message}");
            }
        }

    
        
        /// <summary>
        /// Загружает логотип текущего канала через channels.json.
        /// Обновляет channelLogoImage.
        /// </summary>
        public IEnumerator FetchChannelLogo()
        {
            string url = "https://somafm.com/channels.json";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchChannelLogo failed: {request.error}");
                yield break;
            }

            try
            {
                ChannelListResponse data = JsonUtility.FromJson<ChannelListResponse>(request.downloadHandler.text);

                foreach (ChannelEntry ch in data.channels)
                {
                    if (ch.id != _currentChannelId)
                        continue;

                    // SomaFM отдаёт несколько размеров; берём наибольший
                    string logoUrl = ch.largeimage ?? ch.image ?? ch.xlimage;

                    if (!string.IsNullOrEmpty(logoUrl))
                        StartCoroutine(DownloadAndApplyLogo(logoUrl));

                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FetchChannelLogo parse error: {e.Message}");
            }
        }

        private IEnumerator DownloadAndApplyLogo(string logoUrl)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(logoUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"DownloadAndApplyLogo failed: {request.error}");
                yield break;
            }

            State.ChannelLogo.Value = DownloadHandlerTexture.GetContent(request);
        }

        // ─────────────────────────────────────────────
        // 5. Количество слушателей
        // ─────────────────────────────────────────────

        /// <summary>
        /// Получает количество текущих слушателей канала.
        /// Обновляет listenerCountText.
        /// </summary>
        private IEnumerator FetchListenerCount()
        {
            string url = "https://somafm.com/channels.json";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"FetchListenerCount failed: {request.error}");
                yield break;
            }

            try
            {
                ChannelListResponse data = JsonUtility.FromJson<ChannelListResponse>(request.downloadHandler.text);

                foreach (ChannelEntry ch in data.channels)
                {
                    if (ch.id != _currentChannelId)
                        continue;

                    State.ListenerCount.Value = $"Слушателей: {ch.listeners}";
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FetchListenerCount parse error: {e.Message}");
            }
        }
        

        #region Polling-корутины (авто-обновление)
        
        private IEnumerator _pollTrackInfo()
        {
            while (true)
            {
                yield return FetchCurrentTrack();
                yield return FetchPreviousTracks();
                yield return new WaitForSeconds(_trackUpdateInterval);
            }
        }

        private IEnumerator _pollListenerCount()
        {
            while (true)
            {
                yield return FetchListenerCount();
                yield return new WaitForSeconds(_listenersUpdateInterval);
            }
        }


        #endregion
        
        #region JSON-models for JsonUtility

        [Serializable] private class SongListResponse { public SongEntry[] songs; }

        [Serializable]
        private class SongEntry
        {
            public string title;
            public string artist;
            public string album;
            public string image;    // URL обложки (может отсутствовать)
            public string date;     // Unix timestamp в виде строки
        }

        [Serializable] private class ChannelListResponse { public ChannelEntry[] channels; }

        [Serializable]
        private class ChannelEntry
        {
            public string id;
            public string title;
            public string description;
            public string image;        // маленький логотип
            public string largeimage;   // средний логотип
            public string xlimage;      // большой логотип
            public int listeners;
            public string genre;
        }
        
        #endregion
        
    }
}