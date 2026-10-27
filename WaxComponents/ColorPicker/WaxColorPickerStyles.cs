namespace WaxComponents.ColorPicker;

/// <summary>
/// Tailwind utilities for WaxColorPicker, kept as constants so the razor file stays readable (same
/// pattern as WaxSpoilerStyles). Colors come only from Theme/tokens.css; the alpha checkerboard uses
/// --color-border so it stays visible over card, popover and muted surfaces in both themes.
/// </summary>
internal static class WaxColorPickerStyles
{
    /// <summary>Transparent/colored square pattern shown under translucent swatches and the alpha track.</summary>
    public const string AlphaChecker =
        "bg-[repeating-conic-gradient(var(--color-border)_0%_25%,transparent_0%_50%)] bg-[length:10px_10px]";

    /// <summary>Picker shell (a fieldset); position and width are appended per variant.</summary>
    public const string Panel =
        "m-0 min-w-0 rounded-2xl border border-border bg-card p-3.5 shadow-xl shadow-black/5";

    public const string SvField =
        "relative h-40 w-full cursor-crosshair touch-none select-none overflow-hidden rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

    public const string SvThumb =
        "pointer-events-none absolute h-3.5 w-3.5 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-white shadow-sm";

    public const string Swatch =
        "inline-block shrink-0 border border-black/10 dark:border-white/10";

    public const string PresetButton =
        "block cursor-pointer transition-transform hover:scale-105 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

    public const string PopoverTrigger =
        "flex h-10 w-full items-center gap-2.5 rounded-xl border border-border bg-card px-3 text-left text-sm text-foreground transition-colors hover:border-foreground/30 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60";

    public const string SwatchTrigger =
        "inline-flex shrink-0 cursor-pointer transition-transform hover:scale-105 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-60";

    public const string FormatTrigger =
        "flex cursor-pointer items-center gap-1 rounded-md px-1 py-0.5 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed";

    public const string FormatMenu =
        "absolute left-0 top-full z-[95] mt-1.5 w-28 rounded-xl border border-border bg-popover p-1 text-popover-foreground shadow-2xl shadow-black/15";

    public const string FormatOption =
        "block w-full cursor-pointer rounded-lg px-3 py-1.5 text-left text-sm transition-colors hover:bg-muted";

    public const string HexInput =
        "w-full min-w-0 bg-transparent font-light text-base tracking-wide text-foreground outline-none disabled:cursor-not-allowed";

    public const string ChannelLabel =
        "text-[10px] uppercase tracking-wide text-muted-foreground";

    /// <summary>Invisible full-screen backdrop used to close the floating panel and the format menu.</summary>
    public const string ClickAway =
        "fixed inset-0 z-[94]";
}
