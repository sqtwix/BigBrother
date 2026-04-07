namespace BigBrother.Application.Utils;

public static class IgnoredProcesses
{
    /*
     IgnoredProcesses - class that contains processes that 
     should be ignored by BigBrotherApp

     IsIgnored - check process is ignored or not
     AddIgnoredProcess - adds process to _ignoredProcesses 
     */

    private static readonly HashSet<string> _ignoredProcesses =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BigBrother.Presentation",
            "BigBrother",
            "ApplicationFrameHost", // оболочка UWP
            "SearchApp",            // поиск Windows
            "ShellExperienceHost"   // интерфейс оболочк"
        
        };

    public static bool IsIgnored(String processName)
    {
       return _ignoredProcesses.Contains(processName);
    }

    public static void AddIgnoredProcess(String processName) 
    { 
        _ignoredProcesses.Add(processName);
    }
}