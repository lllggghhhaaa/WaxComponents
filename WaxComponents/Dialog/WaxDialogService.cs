using Microsoft.AspNetCore.Components;

namespace WaxComponents.Dialog;

/// <summary>
/// Default <see cref="IWaxDialogService"/>: keeps a stack of open dialogs, resolves
/// their <c>ShowAsync</c> tasks through the contexts and notifies the outlet.
/// Registered scoped (one stack per circuit/user flow) by AddWaxComponents.
/// </summary>
public sealed class WaxDialogService : IWaxDialogService
{
    private readonly List<WaxDialogInstance> _dialogs = [];
    private readonly Lock _gate = new();

    public event Action? Changed;

    public IReadOnlyList<WaxDialogInstance> OpenDialogs
    {
        get
        {
            lock (_gate)
            {
                return _dialogs.ToArray();
            }
        }
    }

    public Task<WaxDialogResult<T>> ShowAsync<T>(
        RenderFragment<WaxDialogContext<T>> content,
        WaxDialogOptions? options = null,
        object? state = null) =>
        ShowCoreAsync<T>(content, null, options, state);

    public Task<WaxDialogResult<T>> ShowAsync<T>(
        Type contentComponent,
        WaxDialogOptions? options = null,
        object? state = null)
    {
        if (!typeof(IWaxDialogContent<T>).IsAssignableFrom(contentComponent))
        {
            throw new ArgumentException(
                $"{contentComponent.Name} must implement IWaxDialogContent<{typeof(T).Name}> "
                + "to be used as service-driven dialog content.", nameof(contentComponent));
        }

        return ShowCoreAsync<T>(null, contentComponent, options, state);
    }

    public async Task<bool> ConfirmAsync(WaxConfirmRequest request)
    {
        var result = await ShowCoreAsync(
            EmptyContent<bool>(),
            null,
            new WaxDialogOptions
            {
                Title = request.Title,
                Description = request.Description,
                DescriptionContent = request.DescriptionContent,
                Class = request.Class,
                CloseOnBackdrop = request.CloseOnBackdrop,
                CloseOnEscape = request.CloseOnEscape,
                ConfirmText = request.ConfirmText ?? "Confirm",
                CancelText = request.CancelText ?? "Cancel",
                Danger = request.Danger,
            },
            null,
            true);

        return !result.Cancelled;
    }

    public Task<bool> ConfirmAsync(string title, string description) =>
        ConfirmAsync(new WaxConfirmRequest { Title = title, Description = description });

    public async Task<string?> PromptAsync(WaxPromptRequest request)
    {
        var result = await ShowCoreAsync(
            PromptContent(request),
            null,
            new WaxDialogOptions { UseShell = false, CloseOnBackdrop = false },
            null);

        return result.Cancelled ? null : result.Value;
    }

    public void CloseAll()
    {
        WaxDialogInstance[] snapshot;
        lock (_gate) snapshot = [.. _dialogs];

        foreach (var dialog in snapshot) dialog.Cancel();
    }

    private static RenderFragment<WaxDialogContext<T>> EmptyContent<T>() => _ => _ => { };

    private static RenderFragment<WaxDialogContext<string?>> PromptContent(WaxPromptRequest request) =>
        context => builder =>
        {
            builder.OpenComponent(0, typeof(WaxPromptBody));
            builder.AddAttribute(1, nameof(WaxPromptBody.Context), context);
            builder.AddAttribute(2, nameof(WaxPromptBody.Request), request);
            builder.CloseComponent();
        };

    private Task<WaxDialogResult<T>> ShowCoreAsync<T>(
        RenderFragment<WaxDialogContext<T>>? content,
        Type? contentComponent,
        WaxDialogOptions? options,
        object? state,
        T confirmValue = default!)
    {
        var context = new WaxDialogContext<T> { State = state };
        var effectiveOptions = options ?? new WaxDialogOptions();

        var body = contentComponent is null
            ? BuildFragmentBody(context, content ?? EmptyContent<T>())
            : BuildComponentBody(context, contentComponent);

        Func<Task>? confirm = effectiveOptions.ConfirmText is null
            ? null
            : () =>
            {
                context.Close(confirmValue);
                return Task.CompletedTask;
            };

        var instance = new WaxDialogInstance(effectiveOptions, body, context.Cancel, confirm);

        lock (_gate) _dialogs.Add(instance);

        Changed?.Invoke();
        _ = RemoveWhenResolvedAsync(context, instance);

        return context.Task;
    }

    private async Task RemoveWhenResolvedAsync<T>(WaxDialogContext<T> context, WaxDialogInstance instance)
    {
        await context.Task;

        lock (_gate) _dialogs.Remove(instance);

        Changed?.Invoke();
    }

    private static RenderFragment BuildFragmentBody<T>(
        WaxDialogContext<T> context, RenderFragment<WaxDialogContext<T>> content) => builder =>
    {
        builder.OpenComponent<CascadingValue<WaxDialogContext<T>>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<WaxDialogContext<T>>.Value), context);
        builder.AddAttribute(2, nameof(CascadingValue<WaxDialogContext<T>>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<WaxDialogContext<T>>.ChildContent),
            (RenderFragment)(inner => inner.AddContent(0, content(context))));
        builder.CloseComponent();
    };

    private static RenderFragment BuildComponentBody<T>(WaxDialogContext<T> context, Type component) => builder =>
    {
        builder.OpenComponent<CascadingValue<WaxDialogContext<T>>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<WaxDialogContext<T>>.Value), context);
        builder.AddAttribute(2, nameof(CascadingValue<WaxDialogContext<T>>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<WaxDialogContext<T>>.ChildContent),
            (RenderFragment)(inner =>
            {
                inner.OpenComponent(0, component);
                inner.AddAttribute(1, "Context", context);
                inner.CloseComponent();
            }));
        builder.CloseComponent();
    };
}
