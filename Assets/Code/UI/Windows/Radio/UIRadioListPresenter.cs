using Code.Core.GameLoop;
using TriInspector;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioListPresenter : UIPresenter<UIRadioListView>, ISubscriber
    {
        private enum EState
        {
            Hidden,
            Channel,
            Tracks,
            All
        }

        [SerializeField, ReadOnly] private EState _state;
        
        public void Subscribe()
        {
            view.ButtonAllChannels.SubscribeToClicked(_onChannelButtonChanged);

            view.ButtonFavChannels.SubscribeToClicked(_onChannelButtonChanged);

          //  view.ImpactChannels.SubscribeToChanged(_onImpactChannelChanged);

            view.ButtonAllTracks.SubscribeToClicked(_onTracksButtonChanged);
           // view.ButtonAllTracks.SubscribeToClicked(_showAllTrack);
            view.ButtonFavTracks.SubscribeToClicked(_onTracksButtonChanged);
            view.ImpactTracks.SubscribeToChanged(_onImpactTracksChanged);
        }

        public void Unsubscribe()
        {
            view.ButtonAllChannels.UnsubscribeFromClicked(_onChannelButtonChanged);

            view.ButtonFavChannels.UnsubscribeFromClicked(_onChannelButtonChanged);

            //view.ImpactChannels.UnsubscribeFromChanged(_onImpactChannelChanged);

            view.ButtonAllTracks.UnsubscribeFromClicked(_onTracksButtonChanged);
            view.ButtonFavTracks.UnsubscribeFromClicked(_onTracksButtonChanged);
            view.ImpactTracks.UnsubscribeFromChanged(_onImpactTracksChanged);
        }

        private void _onChannelButtonChanged()
        {
            switch (_state)
            {
                case EState.Channel:
                case EState.All:
                    break;

                case EState.Hidden:
                default:
                   // view.ImpactChannels.InvokeActiveImpact();
                    _state = EState.Channel;
                    break;

                case EState.Tracks:
                    //view.ImpactChannels.InvokeActiveImpact();
                    _state = EState.All;
                    break;
            }
        }

        private void _onTracksButtonChanged()
        {
            switch (_state)
            {
                case EState.All:
                case EState.Tracks:
                    break;

                case EState.Hidden:
                default:
                    view.ImpactTracks.InvokeActiveImpact();
                    _state = EState.Tracks;
                    break;

                case EState.Channel:
                    view.ImpactTracks.InvokeActiveImpact();
                    _state = EState.All;
                    break;
            }
        }

        private void _onImpactTracksChanged(bool state)
        {
            if (_state is EState.All or EState.Tracks)
            {
                _state = view.DropDownChannels.State.PropertyValue is UIRadioChannelDropDown.EState.None 
                    ? EState.Channel 
                    : EState.Hidden;
            }
        }

        private void _onImpactChannelChanged(bool state)
        {
            if (_state is EState.All or EState.Channel)
            {
                _state = view.ImpactTracks.IsActivated ? EState.Channel : EState.Hidden;
            }
        }

    }
}