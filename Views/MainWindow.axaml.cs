using Avalonia.Controls;
using Avalonia.Interactivity;
using ProjectLauncherTemplate.ViewModels;
using System;

namespace ProjectLauncherTemplate.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SettingsRequested += Vm_SettingsRequested;
            }
        }

        private void Vm_SettingsRequested(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var settingsVm = new SettingsViewModel(vm.GetSettingsService());
                var settingsWindow = new SettingsWindow
                {
                    DataContext = settingsVm
                };
                settingsWindow.ShowDialog(this);
            }
        }

        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private LibVLCSharp.Shared.LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            InitializeVideo();
        }

        private void InitializeVideo()
        {
            try
            {
                _libVLC = new LibVLCSharp.Shared.LibVLC();
                _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

                var videoView = this.FindControl<LibVLCSharp.Avalonia.VideoView>("VideoBackground");
                if (videoView != null)
                {
                    videoView.MediaPlayer = _mediaPlayer;
                }

                // Check for background.mp4 in Assets or local folder
                string videoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "background.mp4");
                
                // Fallback to check just next to exe
                if (!System.IO.File.Exists(videoPath))
                {
                    videoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.mp4");
                }

                if (System.IO.File.Exists(videoPath))
                {
                    using var media = new LibVLCSharp.Shared.Media(_libVLC, videoPath);
                    // Add loop option
                    media.AddOption("input-repeat=65535");
                    _mediaPlayer.Play(media);
                    
                    if (videoView != null)
                    {
                        videoView.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Video Init Failed: {ex.Message}");
            }
        }
    }
}
