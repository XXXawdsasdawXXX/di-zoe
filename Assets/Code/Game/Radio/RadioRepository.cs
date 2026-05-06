using System;
using System.Threading;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Code.Game.Radio
{
    [Preserve]
    public class RadioRepository : IService
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
 
        /// <summary>
        /// Загружает текстуру логотипа канала. Возвращает дефолтный логотип при ошибке.
        /// </summary>
        public async UniTask<Texture2D> FetchLogoAsync(string logoUrl, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(logoUrl))
                return _config.DefaultChannelLogo;
 
            try
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(logoUrl);
                await request.SendWebRequest().WithCancellation(ct);
 
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[RadioRepository] FetchLogo failed: {request.error}");
                    return _config.DefaultChannelLogo;
                }
 
                return DownloadHandlerTexture.GetContent(request);
            }
            catch (OperationCanceledException)
            {
                return _config.DefaultChannelLogo;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RadioRepository] FetchLogo exception: {e.Message}");
                return _config.DefaultChannelLogo;
            }
        }
    }
}