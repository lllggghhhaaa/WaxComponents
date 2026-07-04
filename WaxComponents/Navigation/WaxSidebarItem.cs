using Microsoft.AspNetCore.Components.Routing;

namespace WaxComponents.Navigation;

public record WaxSidebarItem(
    string Href,
    string Label,
    string? Icon = null,
    NavLinkMatch Match = NavLinkMatch.Prefix
);