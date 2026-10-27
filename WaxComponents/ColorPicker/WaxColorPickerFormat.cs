namespace WaxComponents.ColorPicker;

public enum WaxColorPickerFormat
{
    Hex,
    Rgb,
    Hsl,
    Oklch,
}

public static class WaxColorPickerFormatExtensions
{
    public static string ToLabel(this WaxColorPickerFormat format) => format switch
    {
        WaxColorPickerFormat.Hex => "HEX",
        WaxColorPickerFormat.Rgb => "RGB",
        WaxColorPickerFormat.Hsl => "HSL",
        WaxColorPickerFormat.Oklch => "OKLCH",
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };
}
