using Avalonia.Controls;
using ProjectLauncherTemplate.ViewModels;
using System;
using Avalonia.Platform.Storage;

namespace ProjectLauncherTemplate.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is SettingsViewModel vm)
            {
                vm.BrowseRequested += Vm_BrowseRequested;
            }
        }

        private async void Vm_BrowseRequested(object? sender, EventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Game Installation Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0 && DataContext is SettingsViewModel vm)
            {
                vm.SetInstallPath(folders[0].Path.LocalPath);
            }
        }
    }
}
