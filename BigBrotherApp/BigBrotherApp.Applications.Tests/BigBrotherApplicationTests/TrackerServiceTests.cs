using BigBrother.Application.Interfaces;
using BigBrother.Application.Services;
using BigBrother.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace BigBrother.Application.Tests.BigBrotherApplicationTests;

public class TrackerServiceTests
{
    [Fact]
    public async Task GetTotalActiveTimeForDateAsync_ShouldCalculateCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var sessions = new List<ActivitySession>
        {
            new() { StartTime = date.AddHours(10), EndTime = date.AddHours(12) },
            new() { StartTime = date.AddDays(-1).AddHours(20), EndTime = date.AddHours(8) },
            new() { StartTime = date.AddHours(23), EndTime = date.AddDays(1).AddHours(2) },
            new() { StartTime = date.AddHours(14), EndTime = null }
        };

        // Creation moq repository
        var mockRepository = new Mock<IActivitySessionRepository>();
        mockRepository
            .Setup(r => r.GetAllProcessesInDateAsync(date))
            .ReturnsAsync(sessions);

        // Creation moq logger
        var mockLogger = new Mock<ILogger<TrackerService>>();

        // Creation moq of TrackerService
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var total = await service.GetTotalActiveTimeForDateAsync(date);

        // Assert
        Assert.Equal(TimeSpan.FromHours(2 + 8 + 1 + 10), total);
    }

    [Fact]
    public async Task GetSessionsForPeriodAsync_ShouldReturnSessionsInPeriod()
    {
        // Arrange
        var start = new DateTime(2024, 1, 15);
        var end = new DateTime(2024, 1, 20);

        // Создаём тестовые сессии (только те, что должны быть в результате)
        var expectedSessions = new List<ActivitySession>
    {
        new() { StartTime = new DateTime(2024, 1, 16), EndTime = new DateTime(2024, 1, 16, 2, 0, 0) },
        new() { StartTime = new DateTime(2024, 1, 18), EndTime = new DateTime(2024, 1, 18, 3, 0, 0) },
        new() { StartTime = new DateTime(2024, 1, 19), EndTime = null }, // активная сессия
    };

        var mockRepository = new Mock<IActivitySessionRepository>();

        mockRepository
            .Setup(r => r.GetAllProcessesAsync(start, end))
            .ReturnsAsync(expectedSessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetSessionsForPeriodAsync(start, end);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(expectedSessions, result);

        // Проверяем, что метод был вызван с правильными параметрами
        mockRepository.Verify(r => r.GetAllProcessesAsync(start, end), Times.Once);
    }
}
