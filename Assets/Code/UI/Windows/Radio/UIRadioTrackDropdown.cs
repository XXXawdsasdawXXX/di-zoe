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
    public class UIRadioTrackDropdown : UIComponent, IInitializeListener, ISubscriber
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
            _buttonGroup.Checked += _onPressButtonGroup;
        }

        public void Unsubscribe()
        {
            _buttonGroup.Checked -= _onPressButtonGroup;
        }


        private void _initializeAllTracks(RadioSongListModel songs)
        {
            int count = Math.Min(RadioConfiguration.PREVIOUS_TRACKS_COUNT, songs.Songs.Count);
            
            if (songs.Songs == null || count == 0)
            {
                return;
            }

            _all.ClearElements();
            
            for (int i = 1; i < count; i++)
            {
                if (songs.Songs[i] == null)
                {
                    break;
                }
                
                UIRadioSongTab tab = _all.AddElement() as UIRadioSongTab;
                
                if (tab != null)
                {
                    _initializeTrackTab(tab, songs.Songs[i]);
                }
            }
            
            UIRadioSongTab mainTab = _all.UIRadioButton_main as UIRadioSongTab;
           
            if (mainTab != null)
            {
                _initializeTrackTab(mainTab, songs.Songs[0]);
            }
        }
        
   
        private void _initializeFavoriteTracks()
        {
            _fav.ClearElements();

            int index = 0;
            
            List<RadioSongModel> listFav = _radioFavoriteContent.Songs;
            
            foreach (RadioSongModel song in listFav)
            {
                if (_radioFavoriteContent.IsFavoriteSong(song))
                {
                    UIRadioSongTab tab = _fav.AddElement() as UIRadioSongTab;

                    if (tab != null)
                    {
                        tab.SetIndex(index);

                       _initializeTrackTab(tab, song);
                    }
                    else
                    {
                        throw new Exception("UIRadioSongTab is null during favorite songs initialization.");
                    }
                }

                index++;
            }
            
            UIRadioSongTab mainTab = _all.UIRadioButton_main as UIRadioSongTab;
           
            if (mainTab != null)
            {
                _initializeTrackTab(mainTab, _radioTranslation.Model.CurrentSong.PropertyValue);
            }
        }
        
        private void _initializeTrackTab(UIRadioSongTab tab, RadioSongModel song)
        {
            tab.SetModel(new UIRadioSongTab.Model
            {
                Artist = song.artist,
                Title = song.title,
                IsFavorite = _radioFavoriteContent.IsFavoriteSong(song)
            });

            tab.UIRadioButton_Favorite.IsChecked.SubscribeToValue(value =>
            {
                if (value) {_radioFavoriteContent.AddSong(song);}
                else _radioFavoriteContent.RemoveSong(song);
            });
        }
        
        public async UniTask HideView()
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
                _shownAll(_hideDelayCts.Token);
            }
            else
            {
                _shownFav(_hideDelayCts.Token);
            }
        }

        private async void _shownAll(CancellationToken ct)
        {
            _initializeAllTracks(_radioTranslation.Model.PreviousSongs.PropertyValue);

            if (State.PropertyValue is EState.Fav)
            {
                bool isShownList = _fav.IsShownList;
           
                await _fav.HideListView();
                _fav.gameObject.SetActive(false);
                _all.gameObject.SetActive(true);
                
                if (isShownList)
                {
                    _all.ShowListView();
                }
            }
            else
            {
                _all.gameObject.SetActive(true);
                await _impactAll.InvokeActiveImpact();
            }
            
            State.PropertyValue = EState.All;

            bool cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(AUTO_HIDE_DELAY), cancellationToken: ct)
                .SuppressCancellationThrow();

            if (!cancelled)
            {
                await HideView();
            }
        }

        private async void _shownFav(CancellationToken ct)
        {
            _initializeFavoriteTracks();

            if (State.PropertyValue is EState.All)
            {
                bool isShownList = _all.IsShownList;
           
                await _all.HideListView();
                _all.gameObject.SetActive(false);
                _fav.gameObject.SetActive(true);
                
                if (isShownList)
                {
                    _fav.ShowListView();
                }
            }
            else
            {
                _fav.gameObject.SetActive(true);
                await _impactFav.InvokeActiveImpact();
            }
            
            State.PropertyValue = EState.Fav;

            bool cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(AUTO_HIDE_DELAY), cancellationToken: ct)
                .SuppressCancellationThrow();

            if (!cancelled)
            {
                await HideView();
            }
        }
    }
}