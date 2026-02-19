using System.Collections.Generic;
using Code.Core.GameLoop;
using Code.Core.Save.SavedData;
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
        
        private readonly List<IProgressWriter> _progressWriters = new();
        private List<IProgressReader> _progressReader = new();

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

            LoadProgress();

            return UniTask.CompletedTask;
        }

        public void GameExit()
        {
            _saveProgress();
            
        }

        private void _saveProgress()
        {
            foreach (IProgressWriter progressWriter in _progressWriters)
            {
                progressWriter.SaveProgress(_playerProgress);
            }

            PlayerPrefs.SetString(PROGRESS_KEY, _playerProgress.ToJson());

            string data = PlayerPrefs.GetString(PROGRESS_KEY);

 
        }

        private void LoadProgress()
        {
            _playerProgress = PlayerPrefs.GetString(PROGRESS_KEY)?.ToDeserialized<PlayerProgressData>();

            _playerProgress ??= new PlayerProgressData();

            foreach (IProgressReader progressReader in _progressReader)
            {
                progressReader.LoadProgress(_playerProgress);
            }
        }
    }
}