using BigBrother.Application.Interfaces;
using BigBrother.Application.Services;
using BigBrother.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
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
    public async Task GetTotalActiveTimeForDateAsync_ShouldReturnZero()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var sessions = new List<ActivitySession>
        {
            new() { StartTime = date.AddHours(40), EndTime = date.AddHours(45) },
            new() { StartTime = date.AddDays(-5).AddHours(20), EndTime = date.AddHours(-40) },
            new() { StartTime = date.AddHours(50), EndTime = date.AddDays(3).AddHours(2) },
            new() { StartTime = date.AddHours(55), EndTime = null }
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
        Assert.Equal(TimeSpan.FromHours(0), total);
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

        mockRepository.Verify(r => r.GetAllProcessesAsync(start, end), Times.Once);
    }

    [Fact]
    public async Task GetSessionsForPeriodAsync_ShouldReturnZeroSessionsInPeriod()
    {
        // Arrange
        var start = new DateTime(2025, 1, 15);
        var end = new DateTime(2025, 1, 20);

        // Создаём тестовые сессии (только те, что должны быть в результате)
        var emptySessions = new List<ActivitySession>();
        var mockRepository = new Mock<IActivitySessionRepository>();

        mockRepository
            .Setup(r => r.GetAllProcessesAsync(start, end))
            .ReturnsAsync(emptySessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetSessionsForPeriodAsync(start, end);

        // Assert
        Assert.Equal(0, result.Count);
        Assert.Equal(emptySessions, result);

        mockRepository.Verify(r => r.GetAllProcessesAsync(start, end), Times.Once);
    }

    [Fact]
    public async Task GetSessionsSinceSystemStartAsync_ShouldReturnTopProcessesByTotalTime()
    {
        // Arrange
        var start = new DateTime(2024, 1, 15);
        var end = new DateTime(2024, 1, 20);

        var sessions = new List<ActivitySession>
        {
            // Visual Studio: 5 + 3 = 8 h
            new() { ProcessName = "devenv", StartTime = start.AddHours(9), EndTime = start.AddHours(14) },      // 5 часов
            new() { ProcessName = "devenv", StartTime = start.AddHours(15), EndTime = start.AddHours(18) },     // 3 часа
            
            // Chrome: 4 + 2 = 6 h
            new() { ProcessName = "chrome", StartTime = start.AddHours(10), EndTime = start.AddHours(14) },      // 4 часа
            new() { ProcessName = "chrome", StartTime = start.AddHours(16), EndTime = start.AddHours(18) },      // 2 часа
            
            // Spotify: 2 h
            new() { ProcessName = "spotify", StartTime = start.AddHours(11), EndTime = start.AddHours(13) },     // 2 часа
            
            // VS Code: 1 h
            new() { ProcessName = "vscode", StartTime = start.AddHours(12), EndTime = start.AddHours(13) },      // 1 час
        };

        var mockRepository = new Mock<IActivitySessionRepository>();
        mockRepository
            .Setup(r => r.GetAllProcessesAsync(start, end))
            .ReturnsAsync(sessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetTopProcessesAsync(start, end, top: 3);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("devenv", result[0].ProcessName);
        Assert.Equal(TimeSpan.FromHours(8), result[0].TotalTime);
        Assert.Equal("chrome", result[1].ProcessName);
        Assert.Equal(TimeSpan.FromHours(6), result[1].TotalTime);
        Assert.Equal("spotify", result[2].ProcessName);
        Assert.Equal(TimeSpan.FromHours(2), result[2].TotalTime);
    }

    [Fact]
    public async Task GetTopProcessesAsync_ShouldReturnZero()
    {
        // Arrange
        var start = new DateTime(2024, 1, 15);
        var end = new DateTime(2024, 1, 20);

        var sessions = new List<ActivitySession>();

        var mockRepository = new Mock<IActivitySessionRepository>();
        mockRepository
            .Setup(r => r.GetAllProcessesAsync(start, end))
            .ReturnsAsync(sessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetTopProcessesAsync(start, end, top: 3);

        // Assert
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task GetSessionsSinceSystemStartAsync_ShouldReturnProcesses()
    {
        // Arrange
        var systemStart = DateTime.UtcNow.AddHours(-2);
        var now = DateTime.UtcNow;

        var expectedSessions = new List<ActivitySession>
        {
            new() { ProcessName = "devenv", StartTime = systemStart.AddMinutes(10), EndTime = systemStart.AddMinutes(30) },
            new() { ProcessName = "chrome", StartTime = systemStart.AddMinutes(20), EndTime = systemStart.AddMinutes(45) },
            new() { ProcessName = "spotify", StartTime = systemStart.AddMinutes(15), EndTime = systemStart.AddMinutes(25) },
        };

        var mockRepository = new Mock<IActivitySessionRepository>();

        mockRepository
            .Setup(r => r.GetAllProcessesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedSessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetSessionsSinceSystemStartAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(expectedSessions, result);


        mockRepository.Verify(
            r => r.GetAllProcessesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSessionsSinceSystemStartAsync_ShouldReturnZero()
    {
        // Arrange
        var systemStart = DateTime.UtcNow.AddHours(-2);
        var now = DateTime.UtcNow;

        var expectedSessions = new List<ActivitySession>();

        var mockRepository = new Mock<IActivitySessionRepository>();

        mockRepository
            .Setup(r => r.GetAllProcessesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedSessions);

        var mockLogger = new Mock<ILogger<TrackerService>>();
        var service = new TrackerService(mockRepository.Object, mockLogger.Object);

        // Act
        var result = await service.GetSessionsSinceSystemStartAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);

        mockRepository.Verify(
            r => r.GetAllProcessesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Once);
    }
}
