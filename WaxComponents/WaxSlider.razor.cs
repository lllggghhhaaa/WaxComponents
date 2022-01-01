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
}