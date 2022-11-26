using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxFloatWindow
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Style { get; set; } = String.Empty;

    public bool Active;

    public void Show()
    {
        Active = true;
        StateHasChanged();
    }

    public void Hide()
    {
        Active = false;
        StateHasChanged();
    }
}