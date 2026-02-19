using Code.Game.Radio;
using Code.Infrastructure.GameLoop;
using Code.Infrastructure.ServiceLocator;
using Cysharp.Threading.Tasks;

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
            _fmMetadataService.State.CurrentTrack.Subscribe(UpdateCurrentTrack);
            _fmMetadataService.State.PreviousTrack.Subscribe(UpdatePreviousTrack);
            _fmMetadataService.State.ListenerCount.Subscribe(UpdateListenersCount);
        }

      
        public void Unsubscribe()
        {
            _fmMetadataService.State.CurrentTrack.Unsubscibe(UpdateCurrentTrack);
            _fmMetadataService.State.PreviousTrack.Unsubscibe(UpdatePreviousTrack);
            _fmMetadataService.State.ListenerCount.Unsubscibe(UpdateListenersCount);
        }
        
        #endregion
        
        private void UpdateCurrentTrack(string value)
        {
            view.UIText_currentTrack.SetText(value);
        }

        private void UpdatePreviousTrack(string value)
        {
            view.UIText_previousTracks.SetText(value);
        }

        private void UpdateListenersCount(string value)
        {
            view.UIText_listenerCount.SetText(value);
        }
    }
}