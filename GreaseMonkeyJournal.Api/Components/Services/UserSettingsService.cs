using Microsoft.JSInterop;

namespace GreaseMonkeyJournal.Api.Components.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly IJSRuntime _jsRuntime;
    private const string CurrencyCodeKey = "currencyCode";
    private const string DefaultCurrencyCode = "USD";
    private static readonly HashSet<string> SupportedCurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD",
        "EUR",
        "GBP",
        "SEK"
    };

    public UserSettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetCurrencyCodeAsync()
    {
        try
        {
            var storedCurrencyCode = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CurrencyCodeKey);
            if (string.IsNullOrWhiteSpace(storedCurrencyCode))
                return DefaultCurrencyCode;

            return SupportedCurrencyCodes.Contains(storedCurrencyCode) ? storedCurrencyCode.ToUpperInvariant() : DefaultCurrencyCode;
        }
        catch
        {
            return DefaultCurrencyCode;
        }
    }

    public async Task SetCurrencyCodeAsync(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty.", nameof(currencyCode));

        var normalizedCurrencyCode = currencyCode.ToUpperInvariant();
        if (!SupportedCurrencyCodes.Contains(normalizedCurrencyCode))
            throw new ArgumentException("Unsupported currency code.", nameof(currencyCode));

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CurrencyCodeKey, normalizedCurrencyCode);
    }
}
