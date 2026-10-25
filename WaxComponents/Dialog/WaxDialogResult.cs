namespace WaxComponents.Dialog;

/// <summary>
/// Outcome of a service-driven dialog: a value produced by <see cref="WaxDialogContext{T}.Close"/>,
/// or cancellation (cancel button, backdrop, Escape key, CloseAll).
/// </summary>
public sealed record WaxDialogResult<T>(T? Value, bool Cancelled)
{
    internal static WaxDialogResult<T> CancelledResult { get; } = new(default, Cancelled: true);
}
