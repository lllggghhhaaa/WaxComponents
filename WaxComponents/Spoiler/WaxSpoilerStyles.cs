namespace WaxComponents.Spoiler;

/// <summary>
/// Shared Tailwind utility classes for the spoiler (blur + gradient veil) effect.
/// Centralized so WaxSpoiler, WaxChip, WaxReactionChip and WaxSettingRow render
/// identical visuals; the rose veil colors come from the --spoiler-* tokens in
/// Theme/tokens.css. The bare <c>wax-spoiler</c>/<c>wax-spoiler-veil</c> marker
/// classes carry no styles and exist only as styling/testing hooks.
/// </summary>
internal static class WaxSpoilerStyles
{
    /// <summary>Wrapper: positions the veil, groups hover state and rounds the focus ring.</summary>
    public const string Wrapper =
        "wax-spoiler relative cursor-pointer rounded-[calc(var(--radius)_-_8px)] group/wax-spoiler "
        + "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring";

    /// <summary>Hidden state: keeps the blurred content from being selected or copied.</summary>
    public const string Hidden = "select-none";

    /// <summary>Content: animates the blur away when revealing.</summary>
    public const string Content = "transition-[filter] duration-[250ms]";

    /// <summary>Hidden state content: the blur itself.</summary>
    public const string ContentBlurred = "blur-[5px]";

    /// <summary>Gradient veil overlay; fades out when revealed.</summary>
    public const string Veil =
        "wax-spoiler-veil absolute inset-0 flex items-center justify-center pointer-events-none rounded-[inherit] "
        + "transition-opacity duration-[250ms] "
        + "bg-[linear-gradient(to_right,transparent_0%,var(--spoiler-veil-from)_35%,var(--spoiler-veil-to)_100%)]";

    /// <summary>Hidden state veil: visible.</summary>
    public const string VeilVisible = "opacity-100";

    /// <summary>Revealed state veil: faded out.</summary>
    public const string VeilHidden = "opacity-0";

    /// <summary>Eye hint shown while hidden; brightens on wrapper hover.</summary>
    public const string Hint =
        "opacity-75 transition-all duration-[200ms] text-(--spoiler-hint) "
        + "group-hover/wax-spoiler:opacity-100 group-hover/wax-spoiler:text-foreground";
}
