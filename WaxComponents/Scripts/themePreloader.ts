/* Wax theme preloader.
 *
 * Compiled into the ThemePreloaderScript const (see Scripts/build.mjs) and inlined
 * by ThemePreloader.razor so it executes while the <head> parses — before first
 * paint and before any module import — which is what prevents the wrong-theme
 * flash. Keep it self-contained: no imports, no dependencies, no exports.
 *
 * NOTE: the constants below are duplicated in theme.ts — keep them in sync.
 */

const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";
const VALID: readonly string[] = ["system", "light", "dark"];

const root = document.documentElement;

[...root.classList]
    .filter(c => c.startsWith(THEME_PREFIX))
    .forEach(c => root.classList.remove(c));

const stored = localStorage.getItem(STORAGE_KEY)?.toLowerCase();
const theme = stored && VALID.includes(stored) ? stored : "system";

root.classList.add(theme === "dark"
    ? `${THEME_PREFIX}dark`
    : theme === "light" || !window.matchMedia("(prefers-color-scheme: dark)").matches
        ? `${THEME_PREFIX}light`
        : `${THEME_PREFIX}dark`);
