using Microsoft.AspNetCore.Components;

namespace WaxComponents.Dialog;

/// <summary>One open dialog as rendered by <see cref="WaxDialogOutlet"/>.</summary>
public sealed class WaxDialogInstance
{
    private readonly Action _cancel;
    private readonly Func<Task>? _confirm;

    internal WaxDialogInstance(
        WaxDialogOptions options,
        RenderFragment body,
        Action cancel,
        Func<Task>? confirm)
    {
        Options = options;
        Body = body;
        _cancel = cancel;
        _confirm = confirm;
    }

    internal Guid Id { get; } = Guid.NewGuid();

    internal WaxDialogOptions Options { get; }

    /// <summary>The dialog content, with its context already cascaded.</summary>
    internal RenderFragment Body { get; }

    internal void Cancel() => _cancel();

    internal Task ConfirmAsync() => _confirm?.Invoke() ?? Task.CompletedTask;
}
