using BigBrother.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Application.Services;

public class TrackerService : ITrackerService
{
    // Service for tracking sessions and giving info about them


    private readonly ITrackerService _trackerService;

    public TrackerService(ITrackerService trackerService)
    {
        _trackerService = trackerService;
    }
}

