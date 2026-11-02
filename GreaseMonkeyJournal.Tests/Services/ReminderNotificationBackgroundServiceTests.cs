using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GreaseMonkeyJournal.Tests.Services;

public class ReminderNotificationBackgroundServiceTests
{
    // Testable subclass that returns an instant delay so tests don't wait hours.
    private class InstantDispatchService : ReminderNotificationBackgroundService
    {
        public InstantDispatchService(IServiceProvider serviceProvider, ILogger<ReminderNotificationBackgroundService> logger)
            : base(serviceProvider, logger, Options.Create(new AppSettings())) { }

        protected override TimeSpan CalculateDelay() => TimeSpan.FromMilliseconds(1);
    }

    private static (InstantDispatchService service, Mock<INotificationDispatcherService> mockDispatcher) Build(
        Action<Mock<INotificationDispatcherService>>? setup = null)
    {
        var mockDispatcher = new Mock<INotificationDispatcherService>();
        setup?.Invoke(mockDispatcher);

        var services = new ServiceCollection();
        services.AddTransient<INotificationDispatcherService>(_ => mockDispatcher.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockLogger = new Mock<ILogger<ReminderNotificationBackgroundService>>();
        return (new InstantDispatchService(serviceProvider, mockLogger.Object), mockDispatcher);
    }

    [Fact]
    public async Task ExecuteAsync_InvokesDispatcher()
    {
        var (service, mockDispatcher) = Build(m => m.Setup(d => d.SendRemindersAsync()).ReturnsAsync(0));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        mockDispatcher.Verify(d => d.SendRemindersAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_DispatcherThrows_DoesNotPropagateException()
    {
        var (service, _) = Build(m =>
            m.Setup(d => d.SendRemindersAsync()).ThrowsAsync(new Exception("SMTP failed")));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.StartAsync(cts.Token);
            await Task.Delay(100);
            await service.StopAsync(CancellationToken.None);
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task StopAsync_CancelsCleanly()
    {
        var (service, mockDispatcher) = Build(m => m.Setup(d => d.SendRemindersAsync()).ReturnsAsync(0));

        await service.StartAsync(CancellationToken.None);

        var exception = await Record.ExceptionAsync(() => service.StopAsync(CancellationToken.None));

        Assert.Null(exception);
    }
}
