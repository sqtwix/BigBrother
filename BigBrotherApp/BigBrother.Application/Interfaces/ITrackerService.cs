using BigBrother.Domain.Entities;

namespace BigBrother.Application.Interfaces;

public interface ITrackerService
{
    // Inteface for methods of TrackerService

    Task StartTrackingAsync();
    Task StopTrackingAsync();
    void UpdateCurrentSession(object state);
    Task CloseCurrentSessionAsync();
    Task<List<ActivitySession>> GetSessionsForPeriodAsync(DateTime start, DateTime end);
    Task<TimeSpan> GetTotalActiveTimeForDateAsync(DateTime date);
    Task<List<(string ProcessName, TimeSpan TotalTime)>> GetTopProcessesAsync(DateTime start, DateTime end, int top = 5)
}

