using BigBrother.Application.Interfaces;
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

        return await query.ToListAsync();
    }

    // Get session By Id
    public async Task<ActivitySession?> GetProcessById(int processId)
    {
        return await _context.Sessions.FindAsync(new object[] { processId });
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
        var query = _context.Sessions
            .Where(s => (s.StartTime <= date && s.EndTime >= date) ||
            (s.EndTime == null && s.StartTime != null));

        return await query.ToListAsync();
    }

    // Determined savechanges method
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}