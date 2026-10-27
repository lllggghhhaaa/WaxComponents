namespace WaxComponents.Popover;

/// <summary>
/// How the popover opens and closes. Hover keeps the panel open while the pointer is
/// over the trigger or the panel (with a short grace delay across the gap) and also
/// reacts to keyboard focus; Manual leaves opening/closing entirely to the <c>Open</c>
/// parameter or the Show/Hide/Toggle methods. Outside click and Escape close every
/// mode unless disabled.
/// </summary>
public enum WaxPopoverTrigger
{
    Click,
    Hover,
    Focus,
    Manual,
}
