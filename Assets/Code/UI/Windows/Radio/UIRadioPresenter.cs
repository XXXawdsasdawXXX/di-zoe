using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Cysharp.Threading.Tasks;

namespace Code.UI.Windows.Radio
{
    public class UIRadioPresenter : UIPresenter<UIRadioView>, IInitializeListener, ISubscriber
    {
        private SomaFMMetadataService _fmMetadataService;
        private RadioPlayer _radioPlayer;


        #region Life
        
        public UniTask GameInitialize()
        {
            _radioPlayer = Container.Instance.GetService<RadioPlayer>();
            _fmMetadataService = Container.Instance.GetService<SomaFMMetadataService>();
            
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _fmMetadataService.State.CurrentTrack.SubscribeToValue(view.UIText_currentTrack.SetText);
            _fmMetadataService.State.PreviousTrack.SubscribeToValue(view.UIText_previousTracks.SetText);
            _fmMetadataService.State.ListenerCount.SubscribeToValue(view.UIText_listenerCount.SetText);
            _fmMetadataService.State.ChannelLogo.SubscribeToValue(view.UIRawImage_channelLogo.SetTexture);
            
            view.UISlider_volume.SubscribeToElement(_radioPlayer.SetVolume);
            view.UIDropDown_channels.SubscribeToElement(_radioPlayer.ChangeRadioStation);
        }

        public void Unsubscribe()
        {
            _fmMetadataService.State.CurrentTrack.UnsubscibeFromValue(view.UIText_currentTrack.SetText);
            _fmMetadataService.State.PreviousTrack.UnsubscibeFromValue(view.UIText_previousTracks.SetText);
            _fmMetadataService.State.ListenerCount.UnsubscibeFromValue(view.UIText_listenerCount.SetText);
            _fmMetadataService.State.ChannelLogo.UnsubscibeFromValue(view.UIRawImage_channelLogo.SetTexture);
            
            view.UISlider_volume.UnsubscribeFromElement(_radioPlayer.SetVolume);
            view.UIDropDown_channels.UnsubscribeFromElement(_radioPlayer.ChangeRadioStation);
        }
        
        #endregion
        
    }
}