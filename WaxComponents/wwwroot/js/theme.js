/* Wax theme module.
 *
 * Theme model: "light", "dark" or "system". The C# ThemeService (Services/Theme)
 * stores the user *preference*; this module resolves it against the OS preference
 * and applies the resulting class so Tailwind's `dark:` variant (which matches
 * `html.theme-dark`, see Theme/tokens.css) behaves identically in explicit-dark
 * and system modes.
 *
 * NOTE: the constants below are duplicated in ThemePreloader.razor (which must
 * run inline, before any module import) — keep them in sync.
 */
const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";

const media = window.matchMedia("(prefers-color-scheme: dark)");
const root = document.documentElement;

function removeThemeClasses() {
    [...root.classList]
        .filter(c => c.startsWith(THEME_PREFIX))
        .forEach(c => root.classList.remove(c));
}

function resolveTheme(theme) {
    if (!theme || theme.toLowerCase() === "system")
        return media.matches ? "dark" : "light";

    return theme.toLowerCase();
}

function applyClasses(resolved) {
    removeThemeClasses();

    root.classList.add(resolved === "dark"
        ? `${THEME_PREFIX}dark`
        : `${THEME_PREFIX}light`);
}

export function getTheme() {
    return localStorage.getItem(STORAGE_KEY);
}

export function setTheme(theme) {
    localStorage.setItem(STORAGE_KEY, theme);
    applyClasses(resolveTheme(theme));
}

export function getSystemTheme() {
    return media.matches ? "dark" : "light";
}

export function applyTheme(theme) {
    applyClasses(resolveTheme(theme));
}

media.addEventListener("change", () => {
    applyClasses(resolveTheme(localStorage.getItem(STORAGE_KEY)));
});
