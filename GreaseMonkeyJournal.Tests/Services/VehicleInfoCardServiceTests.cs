using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreaseMonkeyJournal.Tests.Services;

public class VehicleInfoCardServiceTests
{
    private VehicleLogDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<VehicleLogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new VehicleLogDbContext(options);

        // Seed a vehicle plus two information cards
        var vehicle = new Vehicle
        {
            Id = 1,
            Make = "Kuhn",
            Model = "GF 8712",
            Year = 2021,
            VehicleType = VehicleType.TractorImplement,
            SerialNumber = "SN-001"
        };
        context.Vehicles.Add(vehicle);

        context.VehicleInfoCards.AddRange(
            new VehicleInfoCard { Id = 1, VehicleId = 1, Title = "Rake", Details = "Article #12345" },
            new VehicleInfoCard { Id = 2, VehicleId = 1, Title = "Tine", Details = "Article #67890" }
        );

        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetByVehicleIdAsync_ReturnsCardsForVehicle()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        var result = await service.GetByVehicleIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(1, c.VehicleId));
        Assert.Contains(result, c => c.Title == "Rake");
        Assert.Contains(result, c => c.Title == "Tine");
    }

    [Fact]
    public async Task GetByVehicleIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByVehicleIdAsync(0));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectCard()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Rake", result.Title);
        Assert.Equal("Article #12345", result.Details);
    }

    [Fact]
    public async Task GetByIdAsync_WithMissingId_ReturnsNull()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsCardToDatabase()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);
        var newCard = new VehicleInfoCard { VehicleId = 1, Title = "Belt", Details = "V-belt SPB 1800" };

        await service.AddAsync(newCard);

        var card = await context.VehicleInfoCards.FirstOrDefaultAsync(c => c.Title == "Belt");
        Assert.NotNull(card);
        Assert.Equal(1, card.VehicleId);
        Assert.Equal("V-belt SPB 1800", card.Details);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCard()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);
        var card = await context.VehicleInfoCards.FindAsync(1);
        Assert.NotNull(card);

        card.Title = "Rake (left)";
        card.Details = "Article #12345-L";

        await service.UpdateAsync(card);

        var updated = await context.VehicleInfoCards.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("Rake (left)", updated.Title);
        Assert.Equal("Article #12345-L", updated.Details);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCardFromDatabase()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        Assert.NotNull(await context.VehicleInfoCards.FindAsync(1));

        await service.DeleteAsync(1);

        Assert.Null(await context.VehicleInfoCards.FindAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_WithMissingId_DoesNotThrow()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        await service.DeleteAsync(999); // should not throw
    }

    [Fact]
    public async Task AddAsync_WithNullCard_ThrowsArgumentNullException()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullCard_ThrowsArgumentNullException()
    {
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<VehicleInfoCardService>>();
        IVehicleInfoCardService service = new VehicleInfoCardService(context, mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }
}
