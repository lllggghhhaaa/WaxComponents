using WaxComponents.Button;

namespace WaxComponents.Dialog;

/// <summary>Input of <see cref="IWaxDialogService.PromptAsync(WaxPromptRequest)"/>.</summary>
public sealed record WaxPromptRequest
{
    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Field label shown above the input.</summary>
    public string Label { get; init; } = string.Empty;

    public string? Placeholder { get; init; }

    /// <summary>Initial value of the input.</summary>
    public string Initial { get; init; } = string.Empty;

    /// <summary>HTML input type of the field ("text", "password", …).</summary>
    public string InputType { get; init; } = "text";

    /// <summary>HTML inputmode hint for the field (e.g. "numeric" for codes).</summary>
    public string? Inputmode { get; init; }

    /// <summary>Optional hard cap on input length.</summary>
    public int? MaxLength { get; init; }

    public string? ConfirmText { get; init; }

    public string? CancelText { get; init; }

    /// <summary>Shorthand for a filled destructive confirm button.</summary>
    public bool Danger { get; init; }

    public string Class { get; init; } = string.Empty;

    /// <summary>
    /// Returns an error line when the current value must not be confirmed
    /// (the dialog stays open showing it), or null to accept.
    /// </summary>
    public Func<string, string?>? Validator { get; init; }

    /// <summary>HTML autocomplete attribute for the field (e.g. "current-password").</summary>
    public string? Autocomplete { get; init; }
}
