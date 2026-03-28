using Microsoft.AspNetCore.Components;

namespace WaxComponents.Buttons;

public partial class WaxButton : ComponentBase
{
    [Parameter] public string?          Label    { get; set; }
    [Parameter] public RenderFragment?  Icon     { get; set; }
    [Parameter] public WaxButtonSize    Size     { get; set; } = WaxButtonSize.Small;
    [Parameter] public WaxButtonType    Type     { get; set; } = WaxButtonType.Round;
    [Parameter] public WaxButtonVariant Variant  { get; set; } = WaxButtonVariant.Filled;
    [Parameter] public WaxButtonColor   Color    { get; set; } = WaxButtonColor.Primary;
    [Parameter] public bool             Disabled { get; set; }
    [Parameter] public EventCallback    OnClick  { get; set; }

    
    private bool ShowIcon  => Icon  is not null;
    private bool ShowLabel => Label  is not null;

    
    private string RootClass =>
        $"wax-btn before:pointer-events-none " +
        $"inline-flex items-center justify-center self-center align-middle " +
        $"min-h-[{MinHeight}] min-w-[48px] select-none " +
        $"relative overflow-hidden " +
        $"before:content-[''] before:absolute before:inset-0 before:opacity-0 " +
        $"{RootRadius} {VariantClass} {StateLayerClass} {DisabledClass} {FocusClass}";

    private string ContainerClass =>
        $"inline-flex items-center justify-center relative z-10 " +
        $"{PaddingClass} {GapClass}";

    //  STATE CLASSES
    private string DisabledClass =>
        Disabled ? "opacity-[0.38] pointer-events-none" : "";

    private string FocusClass =>
        Disabled ? "" :
        "focus-visible:outline focus-visible:outline-2 focus-visible:outline-[var(--color-primary)]";

    //  VARIANT
    private string VariantClass => (Variant, Color) switch
    {
        (WaxButtonVariant.Filled,   WaxButtonColor.Primary)   => "bg-[var(--color-primary)]   text-[var(--color-on-primary)]",
        (WaxButtonVariant.Filled,   WaxButtonColor.Secondary)  => "bg-[var(--color-secondary)]  text-[var(--color-on-secondary)]",
        (WaxButtonVariant.Filled,   WaxButtonColor.Tertiary)   => "bg-[var(--color-tertiary)]   text-[var(--color-on-tertiary)]",

        (WaxButtonVariant.Tonal,    WaxButtonColor.Primary)   => "bg-[var(--color-primary-container)]   text-[var(--color-on-primary-container)]",
        (WaxButtonVariant.Tonal,    WaxButtonColor.Secondary)  => "bg-[var(--color-secondary-container)]  text-[var(--color-on-secondary-container)]",
        (WaxButtonVariant.Tonal,    WaxButtonColor.Tertiary)   => "bg-[var(--color-tertiary-container)]   text-[var(--color-on-tertiary-container)]",

        (WaxButtonVariant.Outlined, WaxButtonColor.Primary)   => "text-[var(--color-primary)]   border border-[var(--color-outline)]",
        (WaxButtonVariant.Outlined, WaxButtonColor.Secondary)  => "text-[var(--color-secondary)]  border border-[var(--color-outline)]",
        (WaxButtonVariant.Outlined, WaxButtonColor.Tertiary)   => "text-[var(--color-tertiary)]   border border-[var(--color-outline)]",

        (WaxButtonVariant.Text,     WaxButtonColor.Primary)   => "text-[var(--color-primary)]",
        (WaxButtonVariant.Text,     WaxButtonColor.Secondary)  => "text-[var(--color-secondary)]",
        (WaxButtonVariant.Text,     WaxButtonColor.Tertiary)   => "text-[var(--color-tertiary)]",

        (WaxButtonVariant.Elevated, WaxButtonColor.Primary) =>
            "bg-[var(--color-surface-container-low)] " +
            "text-[var(--color-primary)] " +
            "shadow-[var(--shadow-elevation-1)] " +
            "transition-shadow duration-[250ms] " +
            "[transition-timing-function:var(--ease-standard)] " +
            "hover:shadow-[var(--shadow-elevation-2)]",
        (WaxButtonVariant.Elevated, WaxButtonColor.Secondary)  => "bg-[var(--color-surface-container-low)] text-[var(--color-secondary)]  shadow-[var(--shadow-elevation-1)]",
        _ => ""
    };

    // ──────────────────────────────────────────────
    //  STATE LAYER  (pseudo ::before via Tailwind)
    // ──────────────────────────────────────────────

    /// <summary>
    /// The ::before pseudo-element acts as the M3 state layer.
    /// Opacity transitions use an "effects" spring (high damping, no bounce).
    /// For Elevated, the box-shadow also transitions with a spatial spring.
    /// </summary>
    private string StateLayerClass => Disabled ? "" : Variant switch
    {
        WaxButtonVariant.Filled   => "before:bg-[var(--color-on-primary)]            " + StateLayerOpacity,
        WaxButtonVariant.Tonal    => "before:bg-[var(--color-on-secondary-container)] " + StateLayerOpacity,
        WaxButtonVariant.Outlined => "before:bg-[var(--color-on-surface)]             " + StateLayerOpacity,
        WaxButtonVariant.Text     => "before:bg-[var(--color-primary)]                " + StateLayerOpacity,
        WaxButtonVariant.Elevated => "before:bg-[var(--color-primary)]                " + StateLayerOpacity
                                   + " hover:shadow-[var(--shadow-elevation-2)]",
        _ => ""
    };

    // Effects spring: fast, high-damped — no overshoot for opacity/color changes.
    private const string StateLayerOpacity =
        "before:transition-opacity " +
        "before:duration-[150ms] " +
        "before:[transition-timing-function:var(--ease-emphasized)] " +
        "hover:before:opacity-[0.08] " +
        "focus-visible:before:opacity-[0.12] " +
        "active:before:opacity-[0.12]";

    // ──────────────────────────────────────────────
    //  SIZE → padding / gap / minHeight / icon / label
    // ──────────────────────────────────────────────
    private string PaddingClass => Size switch
    {
        WaxButtonSize.ExtraSmall => "px-[12px] py-[6px]",
        WaxButtonSize.Small      => "px-[16px] py-[10px]",
        WaxButtonSize.Medium     => "px-[24px] py-[16px]",
        WaxButtonSize.Large      => "px-[48px] py-[32px]",
        WaxButtonSize.ExtraLarge => "px-[64px] py-[48px]",
        _                        => ""
    };

    private string GapClass => Size switch
    {
        WaxButtonSize.ExtraSmall => "gap-[4px]",
        WaxButtonSize.Small      => "gap-[8px]",
        WaxButtonSize.Medium     => "gap-[8px]",
        WaxButtonSize.Large      => "gap-[12px]",
        WaxButtonSize.ExtraLarge => "gap-[16px]",
        _                        => ""
    };

    private string MinHeight => Size switch
    {
        WaxButtonSize.ExtraSmall => "32px",
        WaxButtonSize.Small      => "40px",
        WaxButtonSize.Medium     => "48px",
        WaxButtonSize.Large      => "56px",
        WaxButtonSize.ExtraLarge => "64px",
        _                        => "40px"
    };

    private int IconSize => Size switch
    {
        WaxButtonSize.ExtraSmall => 20,
        WaxButtonSize.Small      => 20,
        WaxButtonSize.Medium     => 24,
        WaxButtonSize.Large      => 32,
        WaxButtonSize.ExtraLarge => 40,
        _                        => 20
    };

    private string LabelClass => Size switch
    {
        WaxButtonSize.ExtraSmall => "text-[14px] leading-[20px] font-medium",
        WaxButtonSize.Small      => "text-[14px] leading-[20px] font-medium",
        WaxButtonSize.Medium     => "text-[16px] leading-[24px] font-medium",
        WaxButtonSize.Large      => "text-[24px] leading-[32px]",
        WaxButtonSize.ExtraLarge => "text-[32px] leading-[40px]",
        _                        => ""
    };

    // ──────────────────────────────────────────────
    //  SHAPE
    // ──────────────────────────────────────────────
    private string RootRadius => Type switch
    {
        WaxButtonType.Round  => "rounded-[100px]",
        WaxButtonType.Square => Size switch
        {
            WaxButtonSize.ExtraSmall => "rounded-[4px]",
            WaxButtonSize.Small      => "rounded-[8px]",
            WaxButtonSize.Medium     => "rounded-[8px]",
            WaxButtonSize.Large      => "rounded-[16px]",
            WaxButtonSize.ExtraLarge => "rounded-[20px]",
            _                        => "rounded-[8px]"
        },
        _ => ""
    };
}

// ──────────────────────────────────────────────────────
//  Enums (unchanged)
// ──────────────────────────────────────────────────────
public enum WaxButtonSize    { ExtraSmall, Small, Medium, Large, ExtraLarge }
public enum WaxButtonType    { Square, Round }
public enum WaxButtonVariant { Filled, Elevated, Tonal, Outlined, Text }
public enum WaxButtonColor   { Primary, Secondary, Tertiary }