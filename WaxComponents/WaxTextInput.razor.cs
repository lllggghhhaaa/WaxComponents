using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxTextInput
{
    [Parameter] 
    public string Value { get; set; } = String.Empty;
    [Parameter]
    public string Placeholder { get; set; } = String.Empty;
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    private async void OnChange(ChangeEventArgs args)
    {
        Value = args.Value?.ToString() ?? String.Empty;
        await ValueChanged.InvokeAsync(Value);
    }
}