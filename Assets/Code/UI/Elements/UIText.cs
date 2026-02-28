using System.Threading;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIText : UIComponent
    {
        [SerializeField] private TMP_Text _component;

        private CancellationTokenSource _cts;
        
        
        public void SetText(string text)
        {
            _component.text = text;
        }
        
        public async UniTask StartTypewrite(string message)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _component.text = message;
            _component.ForceMeshUpdate();
            _component.maxVisibleCharacters = 0;

            int totalCharacters = _component.textInfo.characterCount;

            for (int i = 0; i <= totalCharacters; i++)
            {
                _component.maxVisibleCharacters = i;

                await UniTask.Delay(Mathf.RoundToInt(UIConfiguration.TYPE_WRITE_DELAY * 1000), 
                    cancellationToken: _cts.Token
                );
            }
        }

        public void StopTypewrite()
        {
            _cts?.Cancel();
        }

        private void OnDisable()
        {
            StopTypewrite();
        }
    }
}