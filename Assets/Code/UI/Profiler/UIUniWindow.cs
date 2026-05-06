using Kirurobo;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Profiler
{
    public class UIUniWindow : MonoBehaviour
    {
        [SerializeField] private UniWindowController _uniWindowController;
        [SerializeField] private Toggle _toggleIsTransparent;
        [SerializeField] private Toggle _toggleIsTopmost;
        [SerializeField] private Toggle _toggleIsBottommost;
        [SerializeField] private Toggle _toggleIsZoomed;
        [SerializeField] private Toggle _toggleShouldFitMonitor;
        [SerializeField] private Toggle _toggleAllowDropFiles;

        
        private void Awake()
        {
            _toggleIsTransparent.SetIsOnWithoutNotify(_uniWindowController.isTransparent);
            _toggleIsTopmost.SetIsOnWithoutNotify(_uniWindowController.isTopmost);
            _toggleIsBottommost.SetIsOnWithoutNotify(_uniWindowController.isBottommost);
            _toggleIsZoomed.SetIsOnWithoutNotify(_uniWindowController.isZoomed);
            _toggleShouldFitMonitor.SetIsOnWithoutNotify(_uniWindowController.shouldFitMonitor);
            _toggleAllowDropFiles.SetIsOnWithoutNotify(_uniWindowController.allowDropFiles);
            
            _toggleIsTransparent.onValueChanged.AddListener(value => _uniWindowController.isTransparent = value);
            _toggleIsTopmost.onValueChanged.AddListener(value => _uniWindowController.isTopmost = value);
            _toggleIsBottommost.onValueChanged.AddListener(value => _uniWindowController.isBottommost = value);
            _toggleIsZoomed.onValueChanged.AddListener(value => _uniWindowController.isZoomed = value);
            _toggleShouldFitMonitor.onValueChanged.AddListener(value => _uniWindowController.shouldFitMonitor = value);
            _toggleAllowDropFiles.onValueChanged.AddListener(value => _uniWindowController.allowDropFiles = value);
        }
    }
}