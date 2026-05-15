using System;
using System.Collections.Generic;
using Code.Core.GameLoop;
using Code.Core.ServiceLocator;
using Code.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Core.Save
{
    [Preserve]
    public class SaveLoadService : IService, ILoadListener, IExitListener
    {
        private const string PROGRESS_KEY = "Progress";
        
        private readonly List<IProgressWriter> _progressWriters = new List<IProgressWriter>();
        private List<IProgressReader> _progressReader = new List<IProgressReader>();

        private PlayerProgressData _playerProgress;

        
        public UniTask GameLoad()
        {
            _progressReader = Container.Instance.GetProgressReaders();
      
            foreach (IProgressReader progressReader in _progressReader)
            {
                if (progressReader is IProgressWriter writer)
                {
                    _progressWriters.Add(writer);
                }
            }

            _loadProgress();

            return UniTask.CompletedTask;
        }

        public void GameExit()
        {
            _saveProgress();
        }

        private void _loadProgress()
        {
            _playerProgress = PlayerPrefs.GetString(PROGRESS_KEY)?.ToDeserialized<PlayerProgressData>();

            _playerProgress ??= new PlayerProgressData();
            
            _playerProgress.GameEnterTime = DateTime.UtcNow;

            foreach (IProgressReader progressReader in _progressReader)
            {
                progressReader.LoadProgress(_playerProgress);
            }
        }

        private void _saveProgress()
        {
            _playerProgress.GameExitTime = DateTime.UtcNow;
            
            foreach (IProgressWriter progressWriter in _progressWriters)
            {
                progressWriter.SaveProgress(_playerProgress);
            }
            
            PlayerPrefs.SetString(PROGRESS_KEY, _playerProgress.ToJson());
        }
    }
}