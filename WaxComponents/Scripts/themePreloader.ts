/* Wax theme preloader.
 *
 * Compiled into the ThemePreloaderScript const (see Scripts/build.mjs) and inlined
 * by ThemePreloader.razor so it executes while the <head> parses — before first
 * paint and before any module import — which is what prevents the wrong-theme
 * flash. Keep it self-contained: no imports, no dependencies, no exports.
 *
 * NOTE: the constants below are duplicated in theme.ts — keep them in sync.
 * (THEME_COOKIE must also match ThemeCookie.Name in Theme/ThemeCookie.cs, which
 * the root component reads to SSR the <html> class.)
 */

const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";
const THEME_COOKIE = "wax-theme";
const VALID: readonly string[] = ["system", "light", "dark"];

const root = document.documentElement;

[...root.classList]
    .filter(c => c.startsWith(THEME_PREFIX))
    .forEach(c => root.classList.remove(c));

const stored = localStorage.getItem(STORAGE_KEY)?.toLowerCase();
const theme = stored && VALID.includes(stored) ? stored : "system";

const resolved = theme === "dark"
    ? "dark"
    : theme === "light" || !window.matchMedia("(prefers-color-scheme: dark)").matches
        ? "light"
        : "dark";

root.classList.add(THEME_PREFIX + resolved);

/* Enhanced navigation patches the <html> class attribute from the SSR response.
 * Without this cookie the server response has no class, so it wipes the preloader's
 * class off and paints the wrong theme for a few frames until interop re-applies it.
 * Mirroring the resolved scheme into a cookie lets the root component SSR
 * <html class="theme-…">, making the patch a no-op.
 */
try {
    document.cookie = `${THEME_COOKIE}=${resolved}; path=/; max-age=31536000; samesite=lax`;
} catch {
    // Storage-restricted browsers: SSR falls back to classless <html> (pre-first-paint
    // correction still works because this inline script re-applies it on every load).
}
