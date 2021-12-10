using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxButton
{
    [Parameter]
    public RenderFragment Text { get; set; }

    [Parameter]
    public RenderFragment Icon { get; set; }

    [Parameter]
    public string FontSize { get; set; } = "20px";

    [Parameter]
    public EventHandler? OnClick { get; set; }
}