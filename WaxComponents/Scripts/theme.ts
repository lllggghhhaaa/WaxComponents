/* Wax theme module.
 *
 * Theme model: "light", "dark" or "system". The C# ThemeService (Services/Theme)
 * stores the user *preference*; this module resolves it against the OS preference
 * and applies the resulting class so Tailwind's `dark:` variant (which matches
 * `html.theme-dark`, see Theme/tokens.css) behaves identically in explicit-dark
 * and system modes.
 *
 * The resolved scheme is also mirrored into a cookie that the root component
 * (App.razor) SSRs onto `<html>`. Blazor's enhanced navigation patches the
 * <html> class attribute from the SSR response; without a server-rendered
 * class that patch wipes the applied class off and the page flashes with the
 * wrong theme for a few frames until interop re-applies it.
 *
 * NOTE: the constants below are duplicated in themePreloader.ts (the inline
 * <head> preloader — keep them in sync). THEME_COOKIE must also match
 * ThemeCookie.Name in Theme/ThemeCookie.cs.
 */

const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";
const THEME_COOKIE = "wax-theme";

/** Concrete scheme applied to the document: "light" or "dark". */
type ResolvedTheme = "light" | "dark";

const media = window.matchMedia("(prefers-color-scheme: dark)");
const root = document.documentElement;

function removeThemeClasses(): void {
    [...root.classList]
        .filter(c => c.startsWith(THEME_PREFIX))
        .forEach(c => root.classList.remove(c));
}

function resolveTheme(theme: string | null): ResolvedTheme {
    if (!theme || theme.toLowerCase() === "system")
        return media.matches ? "dark" : "light";

    return theme.toLowerCase() === "dark" ? "dark" : "light";
}

function applyClasses(resolved: ResolvedTheme): void {
    removeThemeClasses();

    root.classList.add(`${THEME_PREFIX}${resolved}`);

    // Keep the SSR cookie in step with the applied class so enhanced navigation
    // renders the same attribute it would patch (no-op) instead of wiping it.
    try {
        document.cookie = `${THEME_COOKIE}=${resolved}; path=/; max-age=31536000; samesite=lax`;
    } catch {
        // Storage-restricted browsers: the server renders a classless <html>;
        // the preloader still corrects the class before first paint on loads.
    }
}

export function getTheme(): string | null {
    return localStorage.getItem(STORAGE_KEY);
}

export function setTheme(theme: string): void {
    localStorage.setItem(STORAGE_KEY, theme);
    applyClasses(resolveTheme(theme));
}

export function getSystemTheme(): ResolvedTheme {
    return media.matches ? "dark" : "light";
}

export function applyTheme(theme: string): void {
    applyClasses(resolveTheme(theme));
}

media.addEventListener("change", () => {
    applyClasses(resolveTheme(localStorage.getItem(STORAGE_KEY)));
});
