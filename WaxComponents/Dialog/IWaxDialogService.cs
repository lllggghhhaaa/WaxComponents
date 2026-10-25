using Microsoft.AspNetCore.Components;

namespace WaxComponents.Dialog;

/// <summary>
/// Opens dialogs imperatively from anywhere in the interactive tree and awaits
/// typed results. A <see cref="WaxDialogOutlet"/> placed in the layout/page renders
/// whatever the service has open; without an outlet nothing is displayed.
/// </summary>
public interface IWaxDialogService
{
    /// <summary>Raised (from any thread) whenever the open-dialog stack changes.</summary>
    event Action? Changed;

    /// <summary>Currently open dialogs, bottom-most first.</summary>
    IReadOnlyList<WaxDialogInstance> OpenDialogs { get; }

    /// <summary>
    /// Opens a dialog whose body is any render fragment. The fragment receives a
    /// typed <see cref="WaxDialogContext{T}"/> and resolves the returned task with
    /// Close/Cancel; <paramref name="state"/> rides along as Context.State.
    /// </summary>
    Task<WaxDialogResult<T>> ShowAsync<T>(
        RenderFragment<WaxDialogContext<T>> content,
        WaxDialogOptions? options = null,
        object? state = null);

    /// <summary>
    /// Opens a dialog whose body is a component. The component must implement
    /// <see cref="IWaxDialogContent{T}"/>; the context (with <paramref name="state"/>)
    /// is assigned to it automatically.
    /// </summary>
    Task<WaxDialogResult<T>> ShowAsync<T>(
        Type contentComponent,
        WaxDialogOptions? options = null,
        object? state = null);

    /// <summary>Standard confirm card; true when confirmed, false when dismissed.</summary>
    Task<bool> ConfirmAsync(WaxConfirmRequest request);

    /// <summary>Standard confirm card; true when confirmed, false when dismissed.</summary>
    Task<bool> ConfirmAsync(string title, string description);

    /// <summary>
    /// Standard single-field card (text, password, …) with optional validation.
    /// The trimmed value, or null when dismissed.
    /// </summary>
    Task<string?> PromptAsync(WaxPromptRequest request);

    /// <summary>Cancels every open dialog.</summary>
    void CloseAll();
}
