using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxTab
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Style { get; set; } = String.Empty;

    private List<WaxTabItem> _tabs = new();

    private WaxTabItem? _activeTab;

    public void RegisterTab(WaxTabItem tab)
    {
        _tabs.Add(tab);

        tab.SetAsLast();
        if (_tabs.Count == 1)
        {
            tab.SetAsFirst();
            SetTab(tab);
        }
        else
            _tabs[^2].SetAsLast(true);
    }

    public void SetTab(WaxTabItem tab)
    {
        _activeTab?.ChangeActive(false);
        tab.ChangeActive(true);
        
        _activeTab = tab;
        StateHasChanged();
    }
}