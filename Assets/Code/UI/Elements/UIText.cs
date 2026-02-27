using System.Threading;
using Code.UI.Models;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class UIText : UIComponent
    {
        [SerializeField] private TMP_Text _component;

        [SerializeField, ReadOnly] private bool _isTyping;
        
        private CancellationTokenSource _cts;
        
        
        
        public void SetText(string text)
        {
            _component.text = text;
        }
        
        public async UniTask StartTypewrite(string message)
        {
            Debug.Log("Start typewrite");
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _isTyping = true;

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

            _isTyping = false;
        }

        public void StopTypewrite()
        {
            _cts?.Cancel();
            
            _isTyping = false;
        }

        private void OnDisable()
        {
            StopTypewrite();
        }
    }
}