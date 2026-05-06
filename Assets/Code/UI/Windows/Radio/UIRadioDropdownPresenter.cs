using System;
using Code.Core.GameLoop;
using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioDropdownPresenter : UIPresenter<UIRadioDropDownView>, ISubscriber
    {
        private enum EState
        {
            Hidden,
            Channel,
            Tracks
        }

        [SerializeField, ReadOnly] private EState _state;


        public void Subscribe()
        {
            view.ButtonAllChannels.SubscribeToEntered(_onChannelButtonChanged);
            view.ButtonFavChannels.SubscribeToEntered(_onChannelButtonChanged);
            view.ImpactChannels.SubscribeToChanged(_onImpactChannelChanged);
                
            view.ButtonAllTracks.SubscribeToEntered(_onTracksButtonChanged);
            view.ButtonFavTracks.SubscribeToEntered(_onTracksButtonChanged);
            view.ImpactTracks.SubscribeToChanged(_onImpactTracksChanged);
        }

        
        public void Unsubscribe()
        {
            view.ButtonAllChannels.UnsubscribeFromEntered(_onChannelButtonChanged);
            view.ButtonFavChannels.UnsubscribeFromEntered(_onChannelButtonChanged);
            view.ImpactChannels.UnsubscribeFromChanged(_onImpactChannelChanged);
            
            view.ButtonAllTracks.UnsubscribeFromEntered(_onTracksButtonChanged);
            view.ButtonFavTracks.UnsubscribeFromEntered(_onTracksButtonChanged);
            view.ImpactTracks.UnsubscribeFromChanged(_onImpactTracksChanged);
        }

        private void _onChannelButtonChanged(bool entered)
        {
            switch (_state)
            {
                case EState.Hidden:
                default:
                    if (entered)
                    {
                        view.ImpactChannels.InvokeActiveImpact();
                        _state = EState.Channel;
                    }

                    break;

                case EState.Channel:
                    break;

                case EState.Tracks:
                    if (entered)
                    {
                        view.ImpactTracks.InvokeDisableImpact();
                        view.ImpactChannels.InvokeActiveImpact();
                        _state = EState.Channel;
                    }
                    break;
            }
        }

        private void _onTracksButtonChanged(bool entered)
        {
            switch (_state)
            {
                case EState.Hidden:
                default:
                    if (entered)
                    {
                        view.ImpactTracks.InvokeActiveImpact();
                        _state = EState.Tracks;
                    }

                    break;

                case EState.Channel:
                    if (entered)
                    {
                        view.ImpactChannels.InvokeDisableImpact();
                        view.ImpactTracks.InvokeActiveImpact();
                        _state = EState.Tracks;
                    }
                    break;

                case EState.Tracks:
                    break;
            }
        }
        
        private void _onImpactTracksChanged(bool state)
        {
            if (_state is EState.Tracks)
            {
                _state = view.ImpactChannels.IsActivated ? EState.Channel : EState.Hidden;
            }
        }

        private void _onImpactChannelChanged(bool state)
        {
            if (_state is EState.Channel)
            {
                _state = view.ImpactTracks.IsActivated ? EState.Channel : EState.Hidden;
            }
        }
    }
}