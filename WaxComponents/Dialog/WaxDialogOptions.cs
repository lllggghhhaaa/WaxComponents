using Microsoft.AspNetCore.Components;
using WaxComponents.Button;

namespace WaxComponents.Dialog;

/// <summary>
/// Declarative shell and behavior options for service-driven dialogs. Everything is
/// optional: with the defaults the outlet wraps the content in a standard WaxDialog
/// card (title, description, cancel/confirm footer) centered over a backdrop.
/// </summary>
/// <remarks>
/// Set <see cref="UseShell"/> to false to render the content raw — for fully custom
/// overlays the content then brings its own chrome (it may still use WaxDialog).
/// Content that needs a typed confirm button either sets <see cref="ConfirmText"/>
/// (confirmed dialogs close with their default value) or supplies its own
/// <see cref="FooterContent"/> closing through the context.
/// </remarks>
public sealed record WaxDialogOptions
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Replaces the plain description paragraph with rich content.</summary>
    public RenderFragment? DescriptionContent { get; init; }

    /// <summary>Extra classes merged onto the dialog card (e.g. custom sizing).</summary>
    public string Class { get; init; } = string.Empty;

    /// <summary>Extra classes on the backdrop overlay (e.g. extra blur or tint).</summary>
    public string BackdropClass { get; init; } = string.Empty;

    /// <summary>Shorthand for a filled destructive confirm button.</summary>
    public bool Danger { get; init; }

    /// <summary>Renders the content without the WaxDialog shell and backdrop.</summary>
    public bool UseShell { get; init; } = true;

    /// <summary>
    /// Modal by default (showModal: top layer, native backdrop, focus trap, blocked
    /// page). False shows a non-modal floating dialog (show) — the page stays
    /// interactive and CloseOnBackdrop/BackdropClass are ignored.
    /// </summary>
    public bool Modal { get; init; } = true;

    /// <summary>Whether clicking the backdrop cancels the dialog (default true).</summary>
    public bool CloseOnBackdrop { get; init; } = true;

    /// <summary>Whether the Escape key cancels the dialog (default true).</summary>
    public bool CloseOnEscape { get; init; } = true;

    /// <summary>Anchors the card near the top instead of centering (long content).</summary>
    public bool AlignTop { get; init; }

    /// <summary>Confirm-button text; leave unset for footerless dialogs.</summary>
    public string? ConfirmText { get; init; }

    /// <summary>Cancel-button text; leave unset to hide the cancel button.</summary>
    public string? CancelText { get; init; }

    public WaxButtonVariant ConfirmVariant { get; init; } = WaxButtonVariant.Filled;

    public bool ConfirmDisabled { get; init; }

    /// <summary>Replaces the default cancel/confirm footer entirely.</summary>
    public RenderFragment? FooterContent { get; init; }
}
