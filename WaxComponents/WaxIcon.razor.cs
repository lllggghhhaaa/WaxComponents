using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxIcon
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string Size { get; set; } = "12px";

    [Parameter] public string Style { get; set; } = String.Empty;
}