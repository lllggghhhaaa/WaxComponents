using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxButton
{
    [Parameter] public RenderFragment? Text { get; set; }

    [Parameter] public RenderFragment? Icon { get; set; }

    [Parameter] public string FontSize { get; set; } = "12px";

    [Parameter] public WaxButtonStyle ButtonStyle { get; set; } = WaxButtonStyle.Outlined;

    [Parameter] public EventHandler<WaxClickedEventArgs>? OnClick { get; set; }
    [Parameter] public string Style { get; set; } = String.Empty;

    [Parameter] public int Id { get; set; }
}

public enum WaxButtonStyle
{
    None,
    Outlined
}

public class WaxClickedEventArgs : EventArgs
{
    public int Id;

    public WaxClickedEventArgs(int id)
    {
        Id = id;
    }
}