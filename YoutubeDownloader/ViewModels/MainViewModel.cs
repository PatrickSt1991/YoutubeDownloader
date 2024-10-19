using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.CreateDashboardViewModel();

    private async Task ShowFFmpegMessageAsync()
    {
        if (FFmpeg.IsAvailable())
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "FFmpeg mist",
            $"""
            FFmpeg is verplicht voor {Program.Name} om te werken.

            Laat dit weten aan Patrick of zet ffmpeg.exe in {Directory.GetCurrentDirectory()}.
            """,
            "Downloaden",
            "Sluiten"
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute("https://ffmpeg.org/download.html");

        if (Application.Current?.ApplicationLifetime?.TryShutdown(3) != true)
            Environment.Exit(3);
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowFFmpegMessageAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            settingsService.Save();

            // Finalize pending updates
            updateService.FinalizeUpdate(false);
        }

        base.Dispose(disposing);
    }
}
