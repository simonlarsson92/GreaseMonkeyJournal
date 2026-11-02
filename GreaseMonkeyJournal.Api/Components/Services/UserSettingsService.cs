using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace GreaseMonkeyJournal.Api.Components.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<UserSettingsService> _logger;
    private const string CurrencyCodeKey = "currencyCode";
    private const string DefaultCurrencyCode = "USD";
    private static readonly HashSet<string> SupportedCurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD",
        "EUR",
        "GBP",
        "SEK"
    };

    public UserSettingsService(IJSRuntime jsRuntime, ILogger<UserSettingsService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
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
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JavaScript interop failed while reading currency code from localStorage");
            return DefaultCurrencyCode;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Currency code read from localStorage was cancelled");
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
