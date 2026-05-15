using Code.Core.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Windows
{
    public class WindowsController : MonoBehaviour, ISubscriber, IStartListener
    {
        [SerializeField] private UIRadioGroup _radioGroup;
        [SerializeField] private UIView[] _views;

        private UIView _currentView;
        

        public void Subscribe()
        {
            _radioGroup.Checked += _onRadioGroupChecked;
        }

        public UniTask GameStart()
        {
            foreach (UIView view in _views)
            {
                view.Close();
            }

            _currentView = _views[0];
            _currentView.Open();
            _radioGroup.SetCheckedWithoutNotify(0);

            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _radioGroup.Checked -= _onRadioGroupChecked;
        }

        private async void _onRadioGroupChecked(int index)
        {
            if (_currentView != null)
            {
                await _currentView.Close();
            }

            if (_views.Length > index)
            {
                _currentView = _views[index];
                _currentView.Open();
            }
        }
    }
}