using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxNavMenu
{
    [Parameter] public string Title { get; set; } = String.Empty;
    [Parameter] public List<NavMenuItem> Items { get; set; } = new();
    
    private int _selected = 0;
    
    private void ButtonSelected(object sender, ClickedEventArgs args)
    {
        _selected = args.Id;
        _navManager.NavigateTo(Items[_selected].Url);
    }
}

public struct NavMenuItem
{
    public string Name;
    public WaxIcon? Icon;
    public string Url;
}