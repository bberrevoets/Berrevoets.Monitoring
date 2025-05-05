using Microsoft.JSInterop;

namespace Berrevoets.Monitoring.Client.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsDarkMode { get; private set; }

    public event Action? OnThemeChanged;

    public async Task InitializeThemeAsync()
    {
        var darkModeValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "darkMode");
        IsDarkMode = darkModeValue == "true";

        await ApplyTheme();
    }

    public async Task ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "darkMode", IsDarkMode.ToString().ToLower());
        await ApplyTheme();

        OnThemeChanged?.Invoke();
    }

    public async Task SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode) return;

        IsDarkMode = isDarkMode;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "darkMode", IsDarkMode.ToString().ToLower());
        await ApplyTheme();

        OnThemeChanged?.Invoke();
    }

    private async Task ApplyTheme()
    {
        if (IsDarkMode)
            await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-bs-theme", "dark");
        else
            await _jsRuntime.InvokeVoidAsync("document.documentElement.removeAttribute", "data-bs-theme");
    }
}
