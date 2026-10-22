namespace WaxComponents.Menu;

/// <summary>One row of a context menu. Reference type so callers can track identity across renders.</summary>
public sealed class WaxMenuItem(
    string label,
    string icon,
    bool danger = false,
    bool disabled = false,
    string? shortcut = null)
{
    public string Label { get; set; } = label;

    /// <summary>Lucide icon name shown at the row's left.</summary>
    public string Icon { get; set; } = icon;

    /// <summary>Destructive styling (red tint).</summary>
    public bool Danger { get; set; } = danger;

    public bool Disabled { get; set; } = disabled;

    /// <summary>Optional keyboard hint (e.g. "⌘C") shown at the row's right.</summary>
    public string? Shortcut { get; set; } = shortcut;
}
