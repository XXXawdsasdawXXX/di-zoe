using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioRepository : IService, IInitializeListener
    {
        [Serializable]
        private class ChannelListResponse
        {
            public RadioChannelModel[] channels;
        }
 
        private RadioConfiguration _config;
 
        public UniTask GameInitialize()
        {
            _config = Container.Instance.GetConfiguration<RadioConfiguration>();
            
            return UniTask.CompletedTask;
        }
 
        /// <summary>
        /// Загружает список всех каналов. Возвращает null при ошибке.
        /// </summary>
        public async UniTask<RadioChannelModel[]> FetchChannelsAsync(CancellationToken ct = default)
        {
            using UnityWebRequest request = UnityWebRequest.Get(RadioConfiguration.CHANNEL_MODELS_URL);
 
            try
            {
                await request.SendWebRequest().WithCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
 
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[RadioRepository] FetchChannels failed: {request.error}");
                return null;
            }
 
            try
            {
                ChannelListResponse response = request.downloadHandler.text
                    .ToDeserialized<ChannelListResponse>();
 
                return response?.channels;
            }
            catch (Exception e)
            {
                Debug.LogException(new Exception($"[RadioRepository] FetchChannels parse error: {e}"));
                return null;
            }
        }
 
        /// <summary>
        /// Загружает список треков для конкретного канала. Возвращает null при ошибке.
        /// </summary>
        public async UniTask<RadioSongListModel> FetchSongsAsync(string channelId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(channelId))
                return null;
 
            string url = RadioConfiguration.GetTrackModelURL(channelId);
            using UnityWebRequest request = UnityWebRequest.Get(url);
 
            try
            {
                await request.SendWebRequest().WithCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
 
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[RadioRepository] FetchSongs failed: {request.error}");
                return null;
            }
 
            try
            {
                return request.downloadHandler.text.ToDeserialized<RadioSongListModel>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RadioRepository] FetchSongs parse error: {e.Message}");
                return null;
            }
        }
 
       private readonly Dictionary<string, Texture2D> _logoCache = new();
    private readonly HashSet<string> _pendingLogos = new();

    // Вызывать после ApplyChannels — прогреть кэш заранее
    public async UniTask PrefetchLogosAsync(RadioChannelModel[] channels, CancellationToken ct = default)
    {
        foreach (var channel in channels)
        {
            string logoUrl = channel.image.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                ? channel.largeimage
                : channel.image;
            
            if (string.IsNullOrEmpty(logoUrl)) continue;
            if (_logoCache.ContainsKey(logoUrl)) continue;
            
            // Загружаем не параллельно, чтобы не спамить запросами
            await FetchLogoAsync(logoUrl, ct);
        }
    }

    public async UniTask<Texture2D> FetchLogoAsync(string logoUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(logoUrl))
            return _config.DefaultChannelLogo;

        // Возвращаем из кэша мгновенно — нет фриза
        if (_logoCache.TryGetValue(logoUrl, out Texture2D cached))
        {
            Debug.Log("return cash");
            return cached;
        }

        // Защита от параллельных запросов на один url
        if (_pendingLogos.Contains(logoUrl))
            return _config.DefaultChannelLogo;

        _pendingLogos.Add(logoUrl);
        try
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(logoUrl);
            await request.SendWebRequest().WithCancellation(ct);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[RadioRepository] FetchLogo failed: {request.error}");
                return _config.DefaultChannelLogo;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            _logoCache[logoUrl] = texture; // кэшируем
            return texture;
        }
        catch (OperationCanceledException) { return _config.DefaultChannelLogo; }
        catch (Exception e)
        {
            Debug.LogWarning($"[RadioRepository] FetchLogo exception: {e.Message}");
            return _config.DefaultChannelLogo;
        }
        finally
        {
            _pendingLogos.Remove(logoUrl);
        }
    }
    }
}