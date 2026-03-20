namespace BigBrother.Application.DTOs;

public class CurrentActivityDto
{
    // Dto for info about current activity

    public string ProcessName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}

