namespace WaxComponents.Theme;

public record WaxColor(string Name)
{
    public static WaxColor Primary => new("primary");
    public static WaxColor Secondary => new("secondary");
    public static WaxColor Accent => new("accent");
    public static WaxColor Muted => new("muted");

    public override string ToString()
        => Name;
}