/* Wax theme module.
 *
 * Theme model: "light", "dark" or "system". The C# ThemeService (Services/Theme)
 * stores the user *preference*; this module resolves it against the OS preference
 * and applies the resulting class so Tailwind's `dark:` variant (which matches
 * `html.theme-dark`, see Theme/tokens.css) behaves identically in explicit-dark
 * and system modes.
 *
 * NOTE: the constants below are duplicated in themePreloader.ts (the inline
 * <head> preloader — keep them in sync.
 */

const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";

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
