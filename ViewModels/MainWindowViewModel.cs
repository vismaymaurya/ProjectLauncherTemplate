using ProjectLauncherTemplate.Models;
using ProjectLauncherTemplate.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLauncherTemplate.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly UpdateService _updateService;
        private readonly GameService _gameService;
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private string _statusText = "Initializing...";

        [ObservableProperty]
        private double _progress = 0;

        [ObservableProperty]
        private bool _isProgressVisible = false;

        [ObservableProperty]
        private bool _isPlayButtonVisible = false;

        [ObservableProperty]
        private bool _isUpdateButtonVisible = false;

        [ObservableProperty]
        private bool _isInstallButtonVisible = false;

        [ObservableProperty]
        private string _gameVersionText = "v0.0.0";

        private GameVersion? _latestVersion;

        public MainWindowViewModel()
        {
            _updateService = new UpdateService();
            _gameService = new GameService();
            _settingsService = new SettingsService();

            GameVersionText = $"Current: v{_settingsService.Settings.LocalVersion}";
            
            // Auto-check on startup
            _ = CheckForUpdatesAsync();
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            StatusText = "Checking for updates...";
            IsProgressVisible = true;
            IsPlayButtonVisible = false;
            IsUpdateButtonVisible = false;
            IsInstallButtonVisible = false;

            _latestVersion = await _updateService.CheckForUpdatesAsync();

            if (_latestVersion != null)
            {
                var localVer = _settingsService.Settings.LocalVersion;
                Console.WriteLine($"[DEBUG] Remote Version: {_latestVersion.Version}");
                Console.WriteLine($"[DEBUG] Local Version: {localVer}");

                if (IsNewerVersion(_latestVersion.Version, localVer))
                {
                    // Check if installed. If not, showing Update is fine, but Install is better UI.
                    if (!_gameService.IsGameInstalled(_settingsService.Settings.InstallPath))
                    {
                        StatusText = "Game not installed";
                        IsInstallButtonVisible = true;
                    }
                    else
                    {
                        StatusText = $"New version available: v{_latestVersion.Version}";
                        IsUpdateButtonVisible = true;
                    }
                }
                else
                {
                    // Versions match (or local is newer)
                    if (!_gameService.IsGameInstalled(_settingsService.Settings.InstallPath))
                    {
                        StatusText = "Game not installed";
                        IsInstallButtonVisible = true;
                    }
                    else
                    {
                        StatusText = "Ready to play";
                        IsPlayButtonVisible = true;
                    }
                }
            }
            else
            {
                StatusText = "Failed to check for updates. Offline mode?";
                // Check if installed anyway
                if (_gameService.IsGameInstalled(_settingsService.Settings.InstallPath))
                {
                    IsPlayButtonVisible = true;
                }
                else
                {
                     // Offline and not installed? Show Install (which will fail) or just wait.
                     // But we can show Install which might retry logic
                     IsInstallButtonVisible = true; 
                }
            }

            IsProgressVisible = false;
        }

        private bool IsNewerVersion(string remote, string local)
        {
            // Simple helper to compare versions. 
            // In a real app, use Version.Parse
            try
            {
                var v1 = new Version(remote);
                var v2 = new Version(local);
                return v1 > v2;
            }
            catch
            {
                return remote != local;
            }
        }

        [RelayCommand]
        private async Task UpdateGameAsync()
        {
            if (_latestVersion == null) return;

            StatusText = "Downloading update...";
            IsUpdateButtonVisible = false;
            IsInstallButtonVisible = false;
            IsPlayButtonVisible = false;
            IsProgressVisible = true;
            Progress = 0;

            try
            {
                var progressReporter = new Progress<double>(p => Progress = p);
                await _updateService.DownloadAndExtractAsync(_latestVersion.BuildUrl, _settingsService.Settings.InstallPath, progressReporter);

                _settingsService.UpdateLocalVersion(_latestVersion.Version);
                GameVersionText = $"Current: v{_latestVersion.Version}";
                StatusText = "Update complete!";
                IsPlayButtonVisible = true;
            }
            catch (Exception ex)
            {
                StatusText = $"Update failed: {ex.Message}";
                // Restore button state based on condition
                // For simplicity, just show Update button or Install button again
                IsUpdateButtonVisible = true; 
            }
            finally
            {
                IsProgressVisible = false;
            }
        }

        [RelayCommand]
        private async Task LaunchGame()
        {
            try
            {
                StatusText = "Launching game...";
                // Small delay to let UI update
                await Task.Delay(500); 
                _gameService.LaunchGame(_settingsService.Settings.InstallPath);
                
                StatusText = "Game Launched!";
                await Task.Delay(3000);
                StatusText = "Ready to play";
            }
            catch (Exception ex)
            {
                StatusText = $"Launch failed: {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            // Logic to open settings window. 
            // Needs generic way to open window or reference to view.
            // For MVVM purity, usually use a service or event.
            // For simplicity here, we might just expose an event or use a messenger.
            // But I'll handle window creation in View for this specific command or use a simple action.
            // Actually, I can use a simpler approach: define a weak action or similar.
            // Let's rely on the View binding to a command that the View handles, OR
            // pass a WindowService. 
            // I'll make this method fire an event or let the View handle the "Settings" button click code-behind to open the specific window.
            // Or I can add a `SettingsRequested` event.
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? SettingsRequested;

        public SettingsService GetSettingsService() => _settingsService;
    }
}
