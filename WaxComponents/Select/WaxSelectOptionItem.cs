namespace WaxComponents.Select;

/// <summary>One rich row of a select dropdown. Reference type so callers can track identity across renders.</summary>
public sealed class WaxSelectOptionItem(
    string value,
    string? label = null,
    string? icon = null,
    string? description = null,
    string? badge = null,
    bool disabled = false)
{
    /// <summary>Raw value parsed into the select's TValue; shown in the trigger when no custom label fits.</summary>
    public string Value { get; set; } = value;

    /// <summary>Display name; falls back to Value when omitted.</summary>
    public string? Label { get; set; } = label;

    /// <summary>Lucide icon name shown at the row's left (and in the trigger when selected).</summary>
    public string? Icon { get; set; } = icon;

    /// <summary>Secondary text under the label.</summary>
    public string? Description { get; set; } = description;

    /// <summary>Small pill at the row's right (e.g. "Popular").</summary>
    public string? Badge { get; set; } = badge;

    public bool Disabled { get; set; } = disabled;

    public string DisplayLabel => Label ?? Value;
}

/// <summary>Declarative WaxSelectOption children register themselves through this host.</summary>
public interface IWaxSelectHost
{
    void Register(WaxSelectOption option);

    void Unregister(WaxSelectOption option);
}
