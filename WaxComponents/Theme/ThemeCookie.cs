using Microsoft.AspNetCore.Http;

namespace WaxComponents.Theme;

/// <summary>
/// Cookie contract mirrored by the JS theme module (Scripts/theme.ts /
/// Scripts/themePreloader.ts — THEME_COOKIE), which writes the *resolved*
/// scheme ("light" / "dark") alongside the localStorage preference.
/// </summary>
/// <remarks>
/// Blazor's enhanced navigation (used whenever the Router is not
/// interactive and pages are statically server-rendered) patches the
/// <c>&lt;html&gt;</c> class attribute from the SSR response. Rendering the
/// current theme class on <c>&lt;html&gt;</c> makes that patch a no-op;
/// otherwise the preloader-applied class is wiped and the page flashes with
/// the wrong theme for a few frames until interop re-applies it.
/// </remarks>
public static class ThemeCookie
{
    /// <summary>Cookie name written by the JS theme module.</summary>
    public const string Name = "wax-theme";

    /// <summary>
    /// The <c>theme-light</c> / <c>theme-dark</c> class for the resolved
    /// scheme, or null when no valid cookie is available (first visit,
    /// storage-restricted browsers, non-HTTP context). The inline
    /// ThemePreloader then still applies the class before first paint.
    /// </summary>
    public static string? ReadThemeClass(HttpContext? http)
    {
        var value = http?.Request.Cookies[Name];

        return value is "dark" or "light" ? $"theme-{value}" : null;
    }
}
