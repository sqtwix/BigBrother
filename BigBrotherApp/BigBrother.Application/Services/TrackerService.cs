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
using BigBrother.Application.DTOs;


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

    // Shows state of timer
    private bool _isTracking;

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
        if (_isTracking)
        {
            return Task.CompletedTask;
        }

        _isTracking = true;
        _timer = new Timer(UpdateCurrentSession, null, 0, 1000);
        return Task.CompletedTask;
    }

    // Inner method for stopping timer
    public async Task StopTrackingAsync() {
        if (!_isTracking) {
            return;
        }

        _isTracking = false;
        _timer?.Dispose();
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
        ActivitySession? sessionToClose = null;
        lock (_lock)
        {
            if (_currentSession != null)
            {
                _currentSession.EndTime = DateTime.UtcNow;
                sessionToClose = _currentSession;
                _currentSession = null;
            }
        }

        if (sessionToClose != null)
        {
            try
            {
                await _activitySessionRepository.UpdateProcess(sessionToClose);
                await _activitySessionRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing session for {ProcessName}", sessionToClose.ProcessName);
            }
        }
    }

    // Periodic save info about session in db every 10 seconds
    private async Task PeriodicSaveAsync()
    {
        if ((DateTime.UtcNow - _lastSaveTime).TotalSeconds >= 10)
        {
            await _activitySessionRepository.SaveChangesAsync();
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

    // Get time of OS work in currenst session
    public Task<TimeSpan> GetSystemUptimeAsync()
    {
        return Task.FromResult(TimeSpan.FromMilliseconds(Environment.TickCount64));
    }

    // Get current activity
    public async Task<CurrentActivityDto> GetCurrentActivityAsync()
    {
        lock (_lock)
        {
            if (_currentSession != null)
            {
                return new CurrentActivityDto
                {
                    ProcessName = _currentSession.ProcessName,
                    WindowTitle = _currentSession.WindowTitle,
                    Duration = DateTime.UtcNow - _currentSession.StartTime
                };
            }
            
            return new CurrentActivityDto();
        }
    }

    // Get total time of activity in mentioned date
    public async Task<TimeSpan?> GetTotalActiveTimeForDateAsync(DateTime date)
    {
        var sessions = await _activitySessionRepository.GetAllProcessesInDateAsync(date);

        if (sessions == null || sessions.Count == 0)
        {
            return TimeSpan.Zero;
        }

        TimeSpan total = TimeSpan.Zero;
        foreach (var s in sessions)
        {
            var end = s.EndTime ?? DateTime.UtcNow;
            var start = s.StartTime;
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            var effectiveStart = start > dayStart ? start : dayStart;
            var effectiveEnd = end < dayEnd ? end : dayEnd;
            if (effectiveEnd > effectiveStart)
            {
                total += effectiveEnd - effectiveStart;
            }
        }
        return total;
    }


    // Getting top 5 processes by activity time
    public async Task<List<(string ProcessName, TimeSpan TotalTime)>> GetTopProcessesAsync(DateTime start,
        DateTime end, int top = 5)
    {
        var sessions = await _activitySessionRepository.GetAllProcessesAsync(start, end);

        if (sessions == null || sessions.Count == 0)
        {
            return new List<(string ProcessName, TimeSpan TotalTime)>();
        }

        var grouped = sessions
       .Where(s => s.EndTime.HasValue)
       .GroupBy(s => s.ProcessName)
       .Select(g => (
           ProcessName: g.Key,
           TotalTime: TimeSpan.FromTicks(g.Sum(s => (s.EndTime!.Value - s.StartTime).Ticks))
       ))
       .OrderByDescending(x => x.TotalTime)
       .Take(top)
       .ToList();

        return grouped;
    }

    // Get all sessions since system start 
    public async Task<List<ActivitySession>> GetSessionsSinceSystemStartAsync()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var systemStart = DateTime.UtcNow - uptime;

        var session = await _activitySessionRepository.GetAllProcessesAsync(systemStart, DateTime.UtcNow);

        return session ?? new List<ActivitySession>();
    }
}