using Microsoft.AspNetCore.Components;

namespace WaxComponents;

public partial class WaxButton : ComponentBase
{
    
}

public enum WaxButtonSize
{
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge
}

public enum WaxButtonVariant
{
    Filled,
    FilledTonal,
    Outlined,
    Text,
    Elevated
}