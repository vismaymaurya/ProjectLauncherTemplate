using ProjectLauncherTemplate.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ProjectLauncherTemplate.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private string _installPath;

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InstallPath = _settingsService.Settings.InstallPath;
        }

        [RelayCommand]
        private void Browse()
        {
            BrowseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SetInstallPath(string path)
        {
            InstallPath = path;
            _settingsService.UpdateInstallPath(path);
        }

        public event EventHandler? BrowseRequested;
    }
}
