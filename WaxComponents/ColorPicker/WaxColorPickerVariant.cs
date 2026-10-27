namespace WaxComponents.ColorPicker;

public enum WaxColorPickerVariant
{
    /// <summary>Panel rendered in place, without a trigger.</summary>
    Inline,

    /// <summary>Bordered trigger button showing the current color; the panel floats below it.</summary>
    Popover,

    /// <summary>Square/circle swatch as trigger; the panel floats centered below it.</summary>
    Swatch,
}
