using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxIcon : ComponentBase
{
    private string IconTypeSuffix => IconType switch
    {
        WaxIconType.Outlined => "outlined",
        WaxIconType.Rounded  => "rounded",
        WaxIconType.Sharp    => "sharp",
        _ => throw new ArgumentOutOfRangeException(nameof(IconType))
    };
}

public enum WaxIconType
{
    Outlined,
    Rounded,
    Sharp
}