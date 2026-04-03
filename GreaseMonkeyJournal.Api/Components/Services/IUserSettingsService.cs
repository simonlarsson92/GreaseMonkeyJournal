namespace GreaseMonkeyJournal.Api.Components.Services;

public interface IUserSettingsService
{
    Task<string> GetCurrencyCodeAsync();
    Task SetCurrencyCodeAsync(string currencyCode);
}
