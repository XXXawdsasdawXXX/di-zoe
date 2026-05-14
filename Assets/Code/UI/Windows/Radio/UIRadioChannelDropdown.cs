using System;
using System.Collections.Generic;
using System.Threading;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Game.Radio;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Windows.Radio
{
    public class UIRadioChannelDropdown : UIComponent, IInitializeListener, IStartListener ,ISubscriber
    {
        public enum EState
        {
            None,
            All,
            Fav
        }

        private const double AUTO_HIDE_DELAY = 120;
        public ReactiveProperty<EState> State { get; } = new(EState.None);

        [SerializeField] private UIRadioGroup _buttonGroup;

        [SerializeField] private UIImpactComponent _impactAll;
        [SerializeField] private UIImpactComponent _impactFav;

        [SerializeField] private UIDropDown _all;
        [SerializeField] private UIDropDown _fav;

        private RadioTranslation _radioTranslation;
        private RadioFavoriteContent _radioFavoriteContent;

        private CancellationTokenSource _hideDelayCts;


        public UniTask GameInitialize()
        {
            _radioTranslation = Container.Instance.GetService<RadioTranslation>();
            _radioFavoriteContent = Container.Instance.GetService<RadioFavoriteContent>();

            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            _radioTranslation.Model.CurrentChannelIndex.SubscribeToValue(_updateCurrentChannel);
            _all.SubscribeToDropDown(_invokeChanged);
            _fav.SubscribeToDropDown(_invokeChanged);
            _buttonGroup.Checked += _onPressButtonGroup;
        }

        public UniTask GameStart()
        {
            _initializeAllChannels();
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _radioTranslation.Model.CurrentChannelIndex.UnsubscibeFromValue(_updateCurrentChannel);
            _all.UnsubscribeFromDropDown(_invokeChanged);
            _fav.UnsubscribeFromDropDown(_invokeChanged);
            _buttonGroup.Checked -= _onPressButtonGroup;
        }
        
        private void _initializeAllChannels()
        {
            int index = 0;
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioTranslation.Model
                         .Channels)
            {
                UIRadioChannelTab tab = _all.AddElement() as UIRadioChannelTab;

                if (tab != null)
                {
                    tab.SetIndex(index);

                    tab.SetModel(new UIRadioChannelTab.Model
                    {
                        Name = channel.Key,
                        Genre = channel.Value.PropertyValue.genre,
                        IsFavorite = _radioFavoriteContent.IsFavoriteChannel(index)
                    });

                    int channelIndex = index;
                    tab.UIRadioButton_Favorite.IsChecked.SubscribeToValue(value =>
                    {
                        if (value) _radioFavoriteContent.AddChannel(channelIndex);
                        else _radioFavoriteContent.RemoveChannel(channelIndex);
                    });

                    index++;
                }
                else
                {
                    throw new Exception("UIRadioChannelTab is null during all channels initialization.");
                }
            }
        }

        private void _initializeFavoriteChannels()
        {
            _fav.ClearElements();

            int index = 0;
            foreach (KeyValuePair<string, ReactiveProperty<RadioChannelModel>> channel in _radioTranslation.Model
                         .Channels)
            {
                if (_radioFavoriteContent.IsFavoriteChannel(index))
                {
                    UIRadioChannelTab tab = _fav.AddElement() as UIRadioChannelTab;

                    if (tab != null)
                    {
                        tab.SetIndex(index);

                        tab.SetModel(new UIRadioChannelTab.Model
                        {
                            Name = channel.Key,
                            Genre = channel.Value.PropertyValue.genre,
                            IsFavorite = _radioFavoriteContent.IsFavoriteChannel(index)
                        });

                        int channelIndex = index;

                        tab.UIRadioButton_Favorite.IsChecked.SubscribeToValue(value =>
                        {
                            if (value) _radioFavoriteContent.AddChannel(channelIndex);
                            else _radioFavoriteContent.RemoveChannel(channelIndex);
                        });
                    }
                    else
                    {
                        throw new Exception("UIRadioChannelTab is null during favorite channels initialization.");
                    }
                }

                index++;
            }
        }

        private void _updateCurrentChannel(int channelIndex)
        {
            Debug.Log("_updateCurrentChannel");
            
            RadioChannelModel channelModel = _radioTranslation.Model.GetCurrentChannel();

            //all
            UIRadioChannelTab mainTab = _all.UIRadioButton_main as UIRadioChannelTab;
            if (mainTab != null)
            {
                mainTab.SetModel(new UIRadioChannelTab.Model
                {
                    Name = channelModel.title,
                    Genre = channelModel.genre,
                    IsFavorite = _radioFavoriteContent.IsFavoriteChannel(channelIndex)
                });
            }

            _all.SetCurrentValueWithoutNotify(channelIndex);

            //fav
            UIRadioChannelTab mainFavTab = _fav.UIRadioButton_main as UIRadioChannelTab;
            if (mainFavTab != null)
            {
                mainFavTab.SetModel(new UIRadioChannelTab.Model
                {
                    Name = channelModel.title,
                    Genre = channelModel.genre,
                    IsFavorite = _radioFavoriteContent.IsFavoriteChannel(channelIndex)
                });
            }

            _fav.SetCurrentValueWithoutNotify(channelIndex);
        }

        private async UniTask _hideChannelView()
        {
            if (State.PropertyValue is EState.None)
            {
                return;
            }
            
            UIImpactComponent activeImpact = State.PropertyValue == EState.All ? _impactAll : _impactFav;
            await activeImpact.InvokeDisableImpact();

            _buttonGroup.SetCheckedWithoutNotify(-1);

            State.PropertyValue = EState.None;
        }

        private void _onPressButtonGroup(int buttonIndex)
        {
            _hideDelayCts?.Cancel();
            _hideDelayCts = new CancellationTokenSource();

            if (buttonIndex == 0)
            {
                _shownAllChannel(_hideDelayCts.Token);
            }
            else
            {
                _shownFavChannel(_hideDelayCts.Token);
            }
        }

        private async void _shownAllChannel(CancellationToken ct)
        {
            if (State.PropertyValue is EState.Fav)
            {
                await _fav.HideListView();
                _fav.gameObject.SetActive(false);
                _all.gameObject.SetActive(true);
            }
            else
            {
                _all.gameObject.SetActive(true);
                await _impactAll.InvokeActiveImpact();
            }
            
            _all.ShowListView();
            
            State.PropertyValue = EState.All;

            bool cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(AUTO_HIDE_DELAY), cancellationToken: ct)
                .SuppressCancellationThrow();

            if (!cancelled)
            {
                await _hideChannelView();
            }
        }

        private async void _shownFavChannel(CancellationToken ct)
        {
            _initializeFavoriteChannels();

            if (State.PropertyValue is EState.All)
            {
                await _all.HideListView();
                _all.gameObject.SetActive(false);
                _fav.gameObject.SetActive(true);
            }
            else
            {
                _fav.gameObject.SetActive(true);
                await _impactFav.InvokeActiveImpact();
            }
            
            _fav.ShowListView();
            
            State.PropertyValue = EState.Fav;

            bool cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(AUTO_HIDE_DELAY), cancellationToken: ct)
                .SuppressCancellationThrow();

            if (!cancelled)
            {
                await _hideChannelView();
            }
        }

        private void _invokeChanged(int channelIndex)
        {
            if (_radioTranslation.Model.CurrentChannelIndex.PropertyValue != channelIndex)
            {
                _radioTranslation.SetChannel(channelIndex);
            }
        }
    }
}