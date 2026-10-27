namespace WaxComponents.QrCode;

/// <summary>
/// Tailwind utilities for WaxQrCode, kept as constants so the razor file stays readable (same
/// pattern as WaxColorPickerStyles). The code itself is an inline SVG: its default fill is
/// var(--color-foreground) so the symbol theme-adapts without JS, and the background stays
/// transparent by default so the quiet zone blends into any surface.
/// </summary>
internal static class WaxQrCodeStyles
{
    /// <summary>SVG root: height follows the viewBox ratio so Class-based widths (w-full, w-40...) never distort the symbol.</summary>
    public const string Svg = "block h-auto max-w-full";
}
