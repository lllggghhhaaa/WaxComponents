namespace WaxComponents.Dialog;

/// <summary>
/// Implemented by components used as dialog content through
/// <c>ShowAsync&lt;T&gt;(typeof(Component), ...)</c>. The context is assigned
/// automatically; the component reads its typed payload from
/// <see cref="WaxDialogContext{T}.State"/> and resolves the dialog with
/// Close/Cancel.
/// </summary>
public interface IWaxDialogContent<T>
{
    public WaxDialogContext<T>? Context { get; set; }
}
