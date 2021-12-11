using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxMenu
{
    [Parameter] public List<MenuItem> Items { get; set; } = new();
    
    private int _selected = 0;
    private RenderFragment ContentSelected => Items[_selected].Render;

    private void ButtonSelected(object sender, ClickedEventArgs args)
    {
        _selected = args.Id;
        StateHasChanged();
    }
}

public struct MenuItem
{
    public string Name;
    public WaxIcon? Icon;
    public RenderFragment Render;
}