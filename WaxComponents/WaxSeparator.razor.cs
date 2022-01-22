using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxSeparator
{
    [Parameter] public string Width { get; set; } = "90%";

    [Parameter] public string Style { get; set; } = String.Empty;
}