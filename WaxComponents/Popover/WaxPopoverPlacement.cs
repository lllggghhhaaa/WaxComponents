namespace WaxComponents.Popover;

/// <summary>
/// Where the popover panel sits relative to its trigger. Start/End align the panel's
/// leading/trailing edge with the trigger; the bare side values center it. When the
/// panel would overflow the viewport on the chosen side it flips to the opposite
/// side (alignment preserved), then shifts to stay inside the viewport.
/// </summary>
public enum WaxPopoverPlacement
{
    TopStart,
    Top,
    TopEnd,
    BottomStart,
    Bottom,
    BottomEnd,
    LeftStart,
    Left,
    LeftEnd,
    RightStart,
    Right,
    RightEnd,
}
