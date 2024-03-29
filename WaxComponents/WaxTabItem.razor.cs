﻿using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxTabItem
{
    [Parameter]
    public RenderFragment? Text { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public RenderFragment? Content { get; set; }

    [CascadingParameter]
    public WaxTab? Parent { get; set; }
    
    [Parameter]
    public string FontSize { get; set; } = "12px";

    [Parameter]
    public string Style { get; set; } = String.Empty;

    private string _bgImage = "white";
    private string _borderTopRight = "0";
    private string _borderTopLeft = "0";
    private string _borderBottomRight = "0";
    private string _borderBottomLeft = "0";

    protected override void OnInitialized()
    {
        Parent?.RegisterTab(this);

        base.OnInitialized();
    }

    public void ChangeActive(bool active) => _bgImage = active ? "var(--component-bg-active) !important" : "var(--component-bg)";

    public void SetAsFirst(bool remove = false)
    {
        _borderTopLeft = remove ? "0" : "5px";
        _borderBottomLeft = remove ? "0" : "5px";
    }

    public void SetAsLast(bool remove = false)
    {
        _borderTopRight = remove ? "0" : "5px";
        _borderBottomRight = remove ? "0" : "5px";
    }

    private void OnTabClicked() => Parent?.SetTab(this);
}