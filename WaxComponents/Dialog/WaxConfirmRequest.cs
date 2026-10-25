using Microsoft.AspNetCore.Components;
using WaxComponents.Button;

namespace WaxComponents.Dialog;

/// <summary>Input of <see cref="IWaxDialogService.ConfirmAsync(WaxConfirmRequest)"/>.</summary>
public sealed record WaxConfirmRequest
{
    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Replaces the plain description paragraph with rich content.</summary>
    public RenderFragment? DescriptionContent { get; init; }

    public string? ConfirmText { get; init; }

    public string? CancelText { get; init; }

    /// <summary>Shorthand for a filled destructive confirm button.</summary>
    public bool Danger { get; init; }

    public string Class { get; init; } = string.Empty;

    public bool CloseOnBackdrop { get; init; } = true;

    public bool CloseOnEscape { get; init; } = true;
}
