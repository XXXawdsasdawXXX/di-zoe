using Code.Game.Radio;
using Code.Infrastructure.GameLoop;
using Code.Infrastructure.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI
{
    public class UIRadioPresenter : UIPresenter<UIRadioView>, IInitializeListener, ISubscriber
    {
        private SomaFMMetadataService _fmMetadataService;

        
        #region Life
        
        public UniTask GameInitialize()
        {
            _fmMetadataService = Container.Instance.GetService<SomaFMMetadataService>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _fmMetadataService.State.CurrentTrack.Subscribe(view.UIText_currentTrack.SetText);
            _fmMetadataService.State.PreviousTrack.Subscribe(view.UIText_previousTracks.SetText);
            _fmMetadataService.State.ListenerCount.Subscribe(view.UIText_listenerCount.SetText);
            _fmMetadataService.State.AlbumCover.Subscribe(view.UIRawImage_albumCover.SetTexture);
            _fmMetadataService.State.ChannelLogo.Subscribe(view.UIRawImage_channelLogo.SetTexture);
        }

        public void Unsubscribe()
        {
            _fmMetadataService.State.CurrentTrack.Unsubscibe(view.UIText_currentTrack.SetText);
            _fmMetadataService.State.PreviousTrack.Unsubscibe(view.UIText_previousTracks.SetText);
            _fmMetadataService.State.ListenerCount.Unsubscibe(view.UIText_listenerCount.SetText);
            _fmMetadataService.State.AlbumCover.Unsubscibe(view.UIRawImage_albumCover.SetTexture);
            _fmMetadataService.State.ChannelLogo.Unsubscibe(view.UIRawImage_channelLogo.SetTexture);
        }
        
        #endregion
        
    }
}