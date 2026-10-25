namespace WaxComponents.Dialog;

/// <summary>
/// The bridge between dialog content and the call that opened it. Content receives
/// the context as a cascading value (or through <see cref="IWaxDialogContent{T}"/>)
/// and resolves the awaiting <c>ShowAsync</c> task with <see cref="Close"/> or
/// <see cref="Cancel"/>. <see cref="State"/> carries any payload the opener passed,
/// untouched and untyped by the library.
/// </summary>
public sealed class WaxDialogContext<T>
{
    private readonly TaskCompletionSource<WaxDialogResult<T>> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task<WaxDialogResult<T>> Task => _completion.Task;

    internal bool IsResolved => _completion.Task.IsCompleted;

    /// <summary>Arbitrary payload provided by the code that opened the dialog.</summary>
    public object? State { get; internal init; }

    /// <summary>Resolves the dialog with a value. Idempotent: later calls are ignored.</summary>
    public void Close(T value) => _completion.TrySetResult(new WaxDialogResult<T>(value, Cancelled: false));

    /// <summary>Resolves the dialog as cancelled. Idempotent: later calls are ignored.</summary>
    public void Cancel() => _completion.TrySetResult(WaxDialogResult<T>.CancelledResult);
}
