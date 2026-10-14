using Microsoft.AspNetCore.Components;

namespace WaxComponents.Tabs;

public record WaxTabItem(string? Label, RenderFragment Content, string? Icon = null)
{
    public bool Disabled { get; init; }

    public RenderFragment? IconContent { get; init; }
}
