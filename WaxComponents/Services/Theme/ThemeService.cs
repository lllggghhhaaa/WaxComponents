using Microsoft.JSInterop;

namespace WaxComponents.Services.Theme;

public sealed class ThemeService(IJSRuntime js)
    : IThemeService, IAsyncDisposable
{
    private const string ModulePath = "./_content/WaxComponents/js/theme.js";

    private readonly SemaphoreSlim _moduleGate = new(1, 1);
    private IJSObjectReference? _module;

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.System;

    public event Action? ThemeChanged;

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is not null)
            return _module;

        await _moduleGate.WaitAsync();
        try
        {
            if (_module is not null)
                return _module;

#pragma warning disable BL0016 // Import failures are relayed through the guarded public boundaries that call this helper.
            var module = await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
#pragma warning restore BL0016

            _module = module;

            return module;
        }
        finally
        {
            _moduleGate.Release();
        }
    }

    public async Task InitializeAsync()
    {
        var theme = ThemeMode.System;

        try
        {
            var module = await GetModuleAsync();

            var value = await module.InvokeAsync<string?>("getTheme");

            if (Enum.TryParse<ThemeMode>(value, true, out var stored))
                theme = stored;

            await module.InvokeVoidAsync("applyTheme", theme.ToString());
        }
        catch (JSDisconnectedException)
        {
            // Circuit/app is gone: nothing to commit, no subscribers to notify.
            return;
        }
        catch (InvalidOperationException)
        {
            // Expected during static SSR/prerendering where IJSRuntime isn't usable —
            // ThemePreloader.razor has already applied the stored theme before hydration.
            // (RendererInfo cannot be ctor-injected into scoped services outside a
            // renderer scope, so the exception classifies the renderer for us.
            // Because we only commit after successful application, this is also
            // safe for genuine interop failures.)
            return;
        }
        catch (JSException)
        {
            // Storage-restricted browsers etc.: degrade gracefully, keep prior state.
            return;
        }
        catch (OperationCanceledException)
        {
            // Circuit teardown mid-interop.
            return;
        }

        // Commit only after the DOM/storage reflect the theme.
        CurrentTheme = theme;
        ThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        try
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync("setTheme", theme.ToString());
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (InvalidOperationException)
        {
            return;
        }
        catch (JSException)
        {
            return;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Commit only after persistence + DOM application succeeded.
        CurrentTheme = theme;
        ThemeChanged?.Invoke();
    }

    public async Task ToggleDarkLightAsync()
    {
        switch (CurrentTheme)
        {
            case ThemeMode.Dark:
                await SetThemeAsync(ThemeMode.Light);
                return;
            case ThemeMode.Light:
                await SetThemeAsync(ThemeMode.Dark);
                return;
            default:
                // System mode: toggle away from whichever scheme the OS currently resolves to.
                try
                {
                    var module = await GetModuleAsync();

                    var system = await module.InvokeAsync<string>("getSystemTheme");

                    await SetThemeAsync(system.Equals("dark", StringComparison.OrdinalIgnoreCase)
                        ? ThemeMode.Light
                        : ThemeMode.Dark);
                }
                catch (JSDisconnectedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (JSException)
                {
                }
                catch (OperationCanceledException)
                {
                }

                break;
        }
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
