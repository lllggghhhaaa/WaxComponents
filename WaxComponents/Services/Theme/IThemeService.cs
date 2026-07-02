namespace WaxComponents.Services.Theme;

public interface IThemeService
{
    ThemeMode CurrentTheme { get; }

    event Action? ThemeChanged;

    Task InitializeAsync();

    Task SetThemeAsync(ThemeMode theme);

    Task ToggleDarkLightAsync();
}