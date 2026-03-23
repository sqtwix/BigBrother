using BigBrother.Domain.Entities;
using BigBrother.Application.DTOs;

namespace BigBrother.Application.Interfaces;


public interface ITrackerService
{
    // Inteface for methods of TrackerService

    // INNER METHODS (backend code, that not used by UI)
    Task StartTrackingAsync();
    Task StopTrackingAsync();
    void UpdateCurrentSession(object state);
    Task CloseCurrentSessionAsync();
    Task PeriodicSaveAsync();

    // EXTERNAL METHODS (methods, that used by UI to get info)
    Task<CurrentActivityDto> GetCurrentActivityAsync(); 

    Task<TimeSpan> GetSystemUptimeAsync();

    Task<List<ActivitySession>> GetSessionsForPeriodAsync(DateTime start, DateTime end);
    Task<TimeSpan> GetTotalActiveTimeForDateAsync(DateTime date);
    Task<List<(string ProcessName, TimeSpan TotalTime)>> GetTopProcessesAsync(DateTime start, DateTime end, int top = 5);

    Task<List<ActivitySession>> GetSessionsSinceSystemStartAsync();
}

