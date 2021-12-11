using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxPanel
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    [Parameter]
    public string Styles { get; set; }
}