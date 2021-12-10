using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxTextInput
{
    [Parameter] 
    public string Value { get; set; }
    
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }
    
    [Parameter]
    public string Placeholder { get; set; }

    public async void OnChange(ChangeEventArgs args)
    {
        Value = args.Value?.ToString() ?? String.Empty;
        await ValueChanged.InvokeAsync(Value);
    }
}