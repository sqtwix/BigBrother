// Entity for storing data about sessions

namespace BigBrother.Domain.Entities;

public class ActivitySessions
{
    public int Id { get; set; }
    public string? ProcessName { get; set; } = string.Empty;
    public string? WindowTitle {  get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero; 

}

