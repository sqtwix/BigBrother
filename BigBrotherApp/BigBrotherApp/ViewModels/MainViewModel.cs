using BigBrother.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BigBrother.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITrackerService _trackerService;

    private readonly DispatcherTimer _uiTimer;

    [ObservableProperty]
    private string _currentProcess = "-";

    [ObservableProperty]
    private string _currentWindow = "-";

    [ObservableProperty]
    private string _currentDuration = "00:00:00";

    [ObservableProperty]
    private string _systemUpTime = "-";

    [ObservableProperty]
    private bool _isTracking;   

    public MainViewModel(ITrackerService trackerService)
    {
        _trackerService = trackerService;

        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _uiTimer.Tick += async (s, e) => await UpdateActivityAsync();
        _uiTimer.Start();
    }

    [RelayCommand(CanExecute = nameof(CanStartTracking))]
    private async Task StartTrackingAsync()
    {
        try
        {
            await _trackerService.StartTrackingAsync();
            IsTracking = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartTrackingAsync failed: {ex.Message}");
            MessageBox.Show("Ошибка в старте");
        }
    }

    private bool CanStartTracking() => !IsTracking;

    [RelayCommand(CanExecute = nameof(CanStopTracking))]
    private async Task StopTrackingAsync()
    {
        await _trackerService.StopTrackingAsync();
        IsTracking = false;
    }

    private bool CanStopTracking() => IsTracking;

    [RelayCommand]
    private async Task RefreshStats()
    {
        // TODO
    }

    public async Task UpdateActivityAsync()
    {
        var activity = await _trackerService.GetCurrentActivityAsync();
        CurrentProcess = string.IsNullOrEmpty(activity.ProcessName) ? "—" : activity.ProcessName;
        CurrentWindow = string.IsNullOrEmpty(activity.WindowTitle) ? "—" : activity.WindowTitle;
        CurrentDuration = activity.Duration.ToString(@"hh\:mm\:ss");

        var uptime = await _trackerService.GetSystemUptimeAsync();
        SystemUpTime = uptime.ToString(@"dd\.hh\:mm\:ss");
    }
}

