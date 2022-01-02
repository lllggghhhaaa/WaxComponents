using System.Drawing;
using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxSlider
{
    [Parameter] public float Value { get; set; }
    [Parameter] public float Min { get; set; }

    [Parameter]
    public float Max { get; set; } = 100;
    [Parameter]
    public float Step { get; set; }
    [Parameter]
    public EventCallback<float> ValueChanged { get; set; }
    
    private async void OnChange(ChangeEventArgs args)
    {
        Value = Convert.ToSingle(args.Value?.ToString() ?? "0");
        await ValueChanged.InvokeAsync(Value);
    }
}