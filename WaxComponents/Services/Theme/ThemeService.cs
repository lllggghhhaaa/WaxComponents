using Microsoft.JSInterop;

namespace WaxComponents.Services.Theme;

public sealed class ThemeService(IJSRuntime js)
    : IThemeService, IAsyncDisposable
{
    private IJSObjectReference? _module;

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.System;

    public event Action? ThemeChanged;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/WaxComponents/js/theme.js");

        return _module;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var module = await GetModuleAsync();

            var value = await module.InvokeAsync<string?>("getTheme");

            if (Enum.TryParse<ThemeMode>(value, true, out var theme))
                CurrentTheme = theme;

            await ApplyAsync();
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        ThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        CurrentTheme = theme;

        try
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync(
                "setTheme",
                CurrentTheme.ToString());

            await ApplyAsync();
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        ThemeChanged?.Invoke();
    }

    public async Task ToggleDarkLightAsync()
    {
        await SetThemeAsync(CurrentTheme == ThemeMode.Dark
            ? ThemeMode.Light
            : ThemeMode.Dark);
    }

    private async Task ApplyAsync()
    {
        var module = await GetModuleAsync();

        await module.InvokeVoidAsync(
            "applyTheme",
            CurrentTheme.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}