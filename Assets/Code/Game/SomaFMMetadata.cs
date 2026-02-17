using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Fetches and displays metadata from SomaFM API:
/// current track, previous tracks, album cover, channel logo, listener count.
/// Attach to the same GameObject as RadioPlayer, or reference it manually.
/// </summary>
public class SomaFMMetadata : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text currentTrackText;       // "Artist - Title"
    public TMP_Text previousTracksText;     // Last 3 tracks
    public TMP_Text listenerCountText;      // "Listeners: 1234"
    public RawImage albumCoverImage;        // Обложка текущего трека
    public RawImage channelLogoImage;       // Логотип канала

    [Header("Settings")]
    [Tooltip("Интервал обновления трека в секундах")]
    public float trackUpdateInterval = 30f;

    [Tooltip("Интервал обновления числа слушателей в секундах")]
    public float listenersUpdateInterval = 60f;

    // Маппинг: индекс дропдауна → ID канала SomaFM
    private string[] channelIds = {
        "defcon",       // DEFCON Radio
        "groovesalad",  // Groove Salad
        "dronezone",    // Drone Zone
        "indiepop"      // Indie Pop Rocks!
    };

    private string currentChannelId = "defcon";
    private Coroutine trackPollingCoroutine;
    private Coroutine listenersPollingCoroutine;

    // ─────────────────────────────────────────────
    // Точка входа: вызывается при смене канала
    // ─────────────────────────────────────────────

    /// <summary>
    /// Запускает обновление метаданных для выбранного канала.
    /// Вызывайте из RadioPlayer.ChangeRadioStation(int index).
    /// </summary>
    public void OnChannelChanged(int dropdownIndex)
    {
        if (dropdownIndex < 0 || dropdownIndex >= channelIds.Length)
        {
            Debug.LogWarning("SomaFMMetadata: некорректный индекс канала");
            return;
        }

        currentChannelId = channelIds[dropdownIndex];

        // Перезапускаем опрос
        if (trackPollingCoroutine != null) StopCoroutine(trackPollingCoroutine);
        if (listenersPollingCoroutine != null) StopCoroutine(listenersPollingCoroutine);

        trackPollingCoroutine = StartCoroutine(PollTrackInfo());
        listenersPollingCoroutine = StartCoroutine(PollListenerCount());

        // Логотип канала грузим один раз при смене
        StartCoroutine(FetchChannelLogo());
    }

    void Start()
    {
        OnChannelChanged(0); // Стартуем с первого канала
    }

    void OnDestroy()
    {
        if (trackPollingCoroutine != null) StopCoroutine(trackPollingCoroutine);
        if (listenersPollingCoroutine != null) StopCoroutine(listenersPollingCoroutine);
    }

    // ─────────────────────────────────────────────
    // 1. Текущий трек
    // ─────────────────────────────────────────────

    /// <summary>
    /// Получает текущий трек через SomaFM songs API.
    /// Обновляет currentTrackText и запускает загрузку обложки.
    /// </summary>
    public IEnumerator FetchCurrentTrack()
    {
        string url = $"https://somafm.com/songs/{currentChannelId}.json";

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

            if (currentTrackText != null)
                currentTrackText.text = trackLine;

            // Загружаем обложку альбома для текущего трека
            if (!string.IsNullOrEmpty(latest.image))
                StartCoroutine(FetchAlbumCover(latest.image));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"FetchCurrentTrack parse error: {e.Message}");
        }
    }

    // ─────────────────────────────────────────────
    // 2. Предыдущие треки
    // ─────────────────────────────────────────────

    /// <summary>
    /// Получает список последних треков (до 5 штук).
    /// Обновляет previousTracksText.
    /// </summary>
    public IEnumerator FetchPreviousTracks(int count = 5)
    {
        string url = $"https://somafm.com/songs/{currentChannelId}.json";

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

            if (previousTracksText != null)
                previousTracksText.text = sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"FetchPreviousTracks parse error: {e.Message}");
        }
    }

    // ─────────────────────────────────────────────
    // 3. Обложка альбома
    // ─────────────────────────────────────────────

    /// <summary>
    /// Загружает изображение обложки альбома по URL из метаданных трека.
    /// Обновляет albumCoverImage.
    /// </summary>
    public IEnumerator FetchAlbumCover(string imageUrl)
    {
        if (albumCoverImage == null || string.IsNullOrEmpty(imageUrl))
            yield break;

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"FetchAlbumCover failed: {request.error}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        albumCoverImage.texture = texture;
    }

    // ─────────────────────────────────────────────
    // 4. Логотип канала
    // ─────────────────────────────────────────────

    /// <summary>
    /// Загружает логотип текущего канала через channels.json.
    /// Обновляет channelLogoImage.
    /// </summary>
    public IEnumerator FetchChannelLogo()
    {
        if (channelLogoImage == null)
            yield break;

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
                if (ch.id != currentChannelId)
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

        channelLogoImage.texture = DownloadHandlerTexture.GetContent(request);
    }

    // ─────────────────────────────────────────────
    // 5. Количество слушателей
    // ─────────────────────────────────────────────

    /// <summary>
    /// Получает количество текущих слушателей канала.
    /// Обновляет listenerCountText.
    /// </summary>
    public IEnumerator FetchListenerCount()
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
                if (ch.id != currentChannelId)
                    continue;

                if (listenerCountText != null)
                    listenerCountText.text = $"Слушателей: {ch.listeners}";

                break;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"FetchListenerCount parse error: {e.Message}");
        }
    }

    // ─────────────────────────────────────────────
    // Polling-корутины (авто-обновление)
    // ─────────────────────────────────────────────

    private IEnumerator PollTrackInfo()
    {
        while (true)
        {
            yield return FetchCurrentTrack();
            yield return FetchPreviousTracks();
            yield return new WaitForSeconds(trackUpdateInterval);
        }
    }

    private IEnumerator PollListenerCount()
    {
        while (true)
        {
            yield return FetchListenerCount();
            yield return new WaitForSeconds(listenersUpdateInterval);
        }
    }

    // ─────────────────────────────────────────────
    // JSON-модели для JsonUtility
    // ─────────────────────────────────────────────

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
}