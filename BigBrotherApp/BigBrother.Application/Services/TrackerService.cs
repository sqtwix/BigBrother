using BigBrother.Application.Interfaces;
using BigBrother.Application.Utils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BigBrother.Domain.Entities;
using BigBrother.Application.Utils;


namespace BigBrother.Application.Services;

public class TrackerService : ITrackerService
{
    // Service for tracking sessions and giving info about them

    // Private field of repository
    private readonly IActivitySessionRepository _activitySessionRepository;

    // Private field of logger for TrackService
    private readonly ILogger<TrackerService> _logger;

    // Timer for control and get times/intervals and etc
    Timer _timer;

    // Lock for locking sensetive parts of code
    private readonly object _lock = new object();

    // Const time of Treshold
    private const int IdleTresholdSeconds = 60;

    // Filed for storing current session
    private ActivitySession? _currentSession;

    // For periodic saving info about session
    private DateTime _lastSaveTime = DateTime.UtcNow;

    public TrackerService(IActivitySessionRepository activitySessionRepository,
                          ILogger<TrackerService> logger)
    {
        _activitySessionRepository = activitySessionRepository;
        _logger = logger;
    }

    // INNER METHODS (backend code, that not used by UI)

    // Inner method fot starting timer
    public Task StartTrackingAsync()
    {
        _timer = new Timer(UpdateCurrentSession, null, 0, 1000);
        return Task.CompletedTask;
    }

    // Inner method for stopping timer
    public async Task StopTrackingAsync() { 
        _timer.Dispose();
        await CloseCurrentSessionAsync();
    }

    // Inner method for updating info about session every time,
    // that mentioned in Timer
    private async void UpdateCurrentSession(object state)
    {
        try
        {
            var (processName, windowTitle) = NativeWinMethods.GetActiveWindowInfo();
            var idleTime = NativeWinMethods.GetIdleTime();
            bool isUserActive = idleTime < TimeSpan.FromSeconds(IdleTresholdSeconds);

            lock (_lock)
            {
                if (!isUserActive || string.IsNullOrEmpty(processName))
                {
                    if (_currentSession != null)
                    {
                        _ = CloseCurrentSessionAsync(); // fire-and-forget
                    }
                    return;
                }

                if (_currentSession == null)
                {
                    _currentSession = new ActivitySession
                    {
                        ProcessName = processName,
                        WindowTitle = windowTitle,
                        StartTime = DateTime.UtcNow
                    };
                    _ = _activitySessionRepository.AddProcessAsync(_currentSession);
                }
                else if (_currentSession.ProcessName != processName ||
                         _currentSession.WindowTitle != windowTitle)
                {
                    _ = CloseCurrentSessionAsync();

                    _currentSession = new ActivitySession
                    {
                        ProcessName = processName,
                        WindowTitle = windowTitle,
                        StartTime = DateTime.UtcNow
                    };
                    _ = _activitySessionRepository.AddProcessAsync(_currentSession);
                }
            }

            await PeriodicSaveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateCurrentSession");
        }
    }

    // Inner method for closing current session
    // and adding EndTime info in db
    private async Task CloseCurrentSessionAsync()
    {
        lock (_lock)
        {
            if (_currentSession != null)
            {
                _currentSession.EndTime = DateTime.UtcNow;
                _ = _activitySessionRepository.UpdateProcessAsync(_currentSession);
                _currentSession = null;
            }
        }
        await _activitySessionRepository.SaveCnahgesAsync();
    }

    // Periodic save info about session in db every 10 seconds
    private async Task PeriodicSaveAsync()
    {
        if ((DateTime.UtcNow - _lastSaveTime).TotalSeconds >= 10)
        {
            await _activitySessionRepository.SaveCnahgesAsync();
            _lastSaveTime = DateTime.UtcNow;
        }
    }

    // EXTERNAL METHODS (methods, that used by UI to get info)

    // Get sessions for mentioned peroid
    public async Task<List<ActivitySession>> GetSessionsForPeriodAsync(DateTime start, 
                                                                 DateTime end)
    {
        return await _activitySessionRepository.GetAllProcessesAsync(start, end);
    }

    //
}