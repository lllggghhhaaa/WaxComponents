using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxTextInput
{
    [Parameter] public string Value { get; set; } = String.Empty;
    [Parameter] public InputType TextType { get; set; } = InputType.Text;
    [Parameter] public string Placeholder { get; set; } = String.Empty;
    [Parameter] public string Pattern { get; set; } = String.Empty;
    [Parameter] public string Style { get; set; } = String.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    private async void OnChange(ChangeEventArgs args)
    {
        Value = args.Value?.ToString() ?? String.Empty;
        await ValueChanged.InvokeAsync(Value);
    }

    public enum InputType
    {
        Text,
        Password,
        Email,
        Number,
        Tel,
        Time,
        Url
    }
}