using System.Collections.Generic;
using System.Linq;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;

namespace Code.Game.Radio
{
    public class RadioState : IService
    {
        // Словарь каналов: ключ — channel.id, значение — реактивная модель
        public Dictionary<string, ReactiveProperty<RadioChannelModel>> Channels { get; } = new();
 
        // Текущий трек (первый из свежего списка)
        public ReactiveProperty<RadioSongModel> CurrentSong { get; } = new(default);
 
        // История треков текущего канала
        public ReactiveProperty<RadioSongListModel> PreviousSongs { get; } = new(default);
 
        // Индекс активного канала в словаре (-1 = не выбран)
        public ReactiveProperty<int> CurrentChannelIndex { get; } = new(-1);
 
        // Громкость радио (−1 = не инициализирована)
        public ReactiveProperty<float> RadioVolume { get; } = new(-1f);
 
        public UniTask GameInitialize() => UniTask.CompletedTask;
 
        /// <summary>
        /// Обновляет или добавляет канал в словарь.
        /// </summary>
        public void ApplyChannels(RadioChannelModel[] channels)
        {
            if (channels == null) return;
 
            foreach (RadioChannelModel channel in channels)
            {
                if (Channels.TryGetValue(channel.id, out ReactiveProperty<RadioChannelModel> existing))
                    existing.PropertyValue = channel;
                else
                    Channels.Add(channel.id, new ReactiveProperty<RadioChannelModel>(channel));
            }
        }
 
        /// <summary>
        /// Обновляет текущий трек и историю.
        /// </summary>
        public void ApplySongs(RadioSongListModel data)
        {
            if (data?.Songs == null || data.Songs.Count == 0) return;
 
            PreviousSongs.PropertyValue = data;
            CurrentSong.PropertyValue   = data.Songs[0];
        }
 
        /// <summary>
        /// Возвращает модель текущего канала или null если индекс невалиден.
        /// </summary>
        public RadioChannelModel GetCurrentChannel()
        {
            int index = CurrentChannelIndex.PropertyValue;
 
            if (index < 0 || index >= Channels.Count)
                return default;
 
            // ElementAt — O(n) для Dictionary, но каналов обычно мало (~20)
            return Channels.ElementAt(index).Value.PropertyValue;
        }
    }
}