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
    public Task StopTrackingAsync() { 
        _timer.Dispose();
        return Task.CompletedTask;
    }

    // Inner method for updating info about session every time,
    // that mentioned in Timer
    private void UpdateCurrentSession(object state)
    {
        try
        {
            lock (_lock)
            { 
            
            }
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex.Message);
        }
    }

    // EXTERNAL METHODS (methods, that used by UI to get info)

    // Get sessions for mentioned peroid
    public async Task<List<ActivitySession>> GetSessionsForPeriodAsync(DateTime start, 
                                                                 DateTime end)
    {
        return await _activitySessionRepository.GetAllProcessesAsync(start, end);
    }


}