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
    public EventHandler<ClickedEventArgs>? OnClick { get; set; }
    [Parameter]
    public string Styles { get; set; }

    [Parameter] 
    public int Id { get; set; }
}

public class ClickedEventArgs : EventArgs
{
    public int Id;

    public ClickedEventArgs(int id)
    {
        Id = id;
    }
}