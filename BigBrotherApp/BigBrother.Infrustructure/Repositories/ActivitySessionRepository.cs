using BigBrother.Application.Interfaces;
using BigBrother.Application.Utils;
using BigBrother.Domain.Entities;
using BigBrother.Infrustructure.Persistance;
using Microsoft.EntityFrameworkCore;


namespace BigBrother.Infrustructure.Repositories;

public class ActivitySessionRepository : IActivitySessionRepository
{
    // Repository for working with sessions
    
    private readonly AppDbContext _context;

    public ActivitySessionRepository(AppDbContext context)
    {
        _context = context;
    }

    // Adding session
    public async Task AddProcessAsync(ActivitySession session)
    {
        // Checking that procces not in ignoredSet
        if (IgnoredProcesses.IsIgnored(session.ProcessName))
        { 
            return; 
        }

        await _context.Sessions.AddAsync(session);
    }

    // Updating existing session
    public Task UpdateProcess(ActivitySession session)
    {
        _context.Sessions.Update(session);
        return Task.CompletedTask;
    }

    // Get all sessions for peroid
    public async Task<List<ActivitySession>> GetAllProcessesAsync(DateTime start, DateTime end)
    {
        var query = _context.Sessions
            .Where(s => s.StartTime >= start && (s.EndTime <= end || s.EndTime == null));

        var result = await query.ToListAsync();
        return result.Where(s => !IgnoredProcesses.IsIgnored(s.ProcessName)).ToList();
    }

    // Get session By Id
    public async Task<ActivitySession?> GetProcessById(int processId)
    {
        var session = await _context.Sessions.FindAsync(processId);
        if (session != null && IgnoredProcesses.IsIgnored(session.ProcessName))
        {
            return null;
        }

        return session;
    }

    // Deleting process by id
    public async Task DeleteProcessAsync(int processId)
    {
        var session = await GetProcessById(processId);

        if (session != null)
        {
            _context.Sessions.Remove(session);
        }
    }

    // Getting all processes in mentioned date
    public async Task<List<ActivitySession>> GetAllProcessesInDateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        var sessions = await _context.Sessions
            .Where(s => s.StartTime < dayEnd && (s.EndTime == null || s.EndTime > dayStart))
            .ToListAsync();

        return sessions.Where(s => !IgnoredProcesses.IsIgnored(s.ProcessName)).ToList();
    }

    // Determined savechanges method
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}