using Microsoft.JSInterop;

namespace WaxComponents.Popover;

/// <summary>
/// Lazy ES-module bridge for WaxPopover's browser helpers: viewport-aware positioning
/// (flip + shift) of the floating panel, trigger interaction listeners and outside
/// click/Escape/scroll bridging back to Blazor. Compiled from Scripts/popover.ts to
/// ./_content/WaxComponents/js/popover.js by the library's esbuild pipeline
/// (Scripts/build.mjs).
/// </summary>
internal sealed class WaxPopoverJs(IJSRuntime jsRuntime) : IAsyncDisposable
{
    public const string ModulePath = "./_content/WaxComponents/js/popover.js";

    private IJSObjectReference? _module;
    private bool _unavailable;

    /// <summary>Invokes a module export; returns <c>default</c> when JS is unavailable or the call fails.</summary>
    public async Task<T?> InvokeAsync<T>(string identifier, params object?[] args)
    {
        if (_unavailable)
        {
            return default;
        }

        try
        {
            _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            return await _module.InvokeAsync<T>(identifier, args);
        }
        catch (JSException)
        {
            return default;
        }
        catch (JSDisconnectedException)
        {
            _unavailable = true;
            return default;
        }
        catch (ObjectDisposedException)
        {
            _unavailable = true;
            return default;
        }
        catch (InvalidOperationException)
        {
            // Prerendering or a disposed circuit: JS interop is not usable right now.
            _unavailable = true;
            return default;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
