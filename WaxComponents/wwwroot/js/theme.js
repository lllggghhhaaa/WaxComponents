const STORAGE_KEY = "theme";
const THEME_PREFIX = "theme-";

function removeThemeClasses(root) {
    [...root.classList]
        .filter(c => c.startsWith(THEME_PREFIX))
        .forEach(c => root.classList.remove(c));
}

export function getTheme() {
    return localStorage.getItem(STORAGE_KEY);
}

export function setTheme(theme) {
    localStorage.setItem(STORAGE_KEY, theme);
}

export function applyTheme(theme) {
    const root = document.documentElement;

    removeThemeClasses(root);

    if (!theme)
        return;

    root.classList.add(`${THEME_PREFIX}${theme.toLowerCase()}`);
}