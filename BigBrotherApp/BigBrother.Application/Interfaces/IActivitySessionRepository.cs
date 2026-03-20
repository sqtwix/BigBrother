// Interface for TrackerRepository

using BigBrother.Domain.Entities;

namespace BigBrother.Application.Interfaces;

public interface IActivitySessionRepository
{
    // Adding session
    Task AddProcessAsync(ActivitySession session);
    
    // Updating existing session
    Task UpdateProcessAsync(ActivitySession session);
    
    // Get all sessions for peroid
    Task<List<ActivitySession>> GetAllProcessesAsync(DateTime start, DateTime end);

    // Get session By Id
    Task<ActivitySession> GetProcessById(int processId);

    // Deleting process by id
    Task DeleteProcessAsync(int processId);
}

