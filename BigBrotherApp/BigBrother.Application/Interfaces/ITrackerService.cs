// Inteface for tracking service

namespace BigBrother.Application.Interfaces;

public interface ITrackerService
{
    Task StartTrackingAsync();
    Task StopTrackingAsync();
}

