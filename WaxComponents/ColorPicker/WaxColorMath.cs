using System.Globalization;

namespace WaxComponents.ColorPicker;

/// <summary>
/// Color conversion and parsing helpers used by <see cref="WaxColorPicker"/>.
/// Contract: hues 0–360, saturation/value/alpha 0–100, RGB channels 0–255, OKLCH lightness 0–1.
/// All string output is culture-invariant.
/// </summary>
internal static class WaxColorMath
{
    private const MidpointRounding Rounding = MidpointRounding.AwayFromZero;
    private const double AchromaticChromaThreshold = 1e-4;

    internal readonly record struct Rgb(int R, int G, int B);

    internal readonly record struct Hsv(double H, double S, double V);

    internal readonly record struct Hsl(int H, int S, int L);

    internal readonly record struct Oklch(double L, double C, double H);

    internal readonly record struct ParsedHexColor(Rgb Rgb, int? AlphaPercent);

    internal static readonly Rgb DefaultRgb = new(59, 130, 246);

    #region HSV / HSL / RGB

    public static Rgb HsvToRgb(double hue, double saturation, double value)
    {
        var s = Math.Clamp(saturation, 0, 100) / 100;
        var v = Math.Clamp(value, 0, 100) / 100;
        var chroma = v * s;
        var (r, g, b) = HueToRgbPrime(hue, chroma);
        var m = v - chroma;

        return new Rgb(ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    public static Rgb HslToRgb(double hue, double saturation, double lightness)
    {
        var s = Math.Clamp(saturation, 0, 100) / 100;
        var l = Math.Clamp(lightness, 0, 100) / 100;
        var chroma = (1 - Math.Abs(2 * l - 1)) * s;
        var (r, g, b) = HueToRgbPrime(hue, chroma);
        var m = l - chroma / 2;

        return new Rgb(ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    public static Hsv RgbToHsv(Rgb rgb)
    {
        var (max, min, hue) = GetRangeAndHue(rgb);
        var delta = max - min;

        return new Hsv(hue, max == 0 ? 0 : delta / max * 100, max * 100);
    }

    public static Hsl RgbToHsl(Rgb rgb)
    {
        var (max, min, hue) = GetRangeAndHue(rgb);
        var delta = max - min;
        var lightness = (max + min) / 2;
        var saturation = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * lightness - 1));

        return new Hsl(
            (int)Math.Round(hue, Rounding) % 360,
            (int)Math.Round(saturation * 100, Rounding),
            (int)Math.Round(lightness * 100, Rounding));
    }

    /// <summary>Maps a hue to the (R', G', B') triple of the standard HSV/HSL hexagon, before adding the minimum.</summary>
    private static (double R, double G, double B) HueToRgbPrime(double hue, double chroma)
    {
        var normalizedHue = NormalizeHue(hue);
        var intermediate = chroma * (1 - Math.Abs(normalizedHue / 60 % 2 - 1));

        return (int)(normalizedHue / 60) switch
        {
            0 => (chroma, intermediate, 0),
            1 => (intermediate, chroma, 0),
            2 => (0, chroma, intermediate),
            3 => (0, intermediate, chroma),
            4 => (intermediate, 0, chroma),
            _ => (chroma, 0, intermediate),
        };
    }

    /// <summary>Normalized (0–1) max/min channel and the hue in degrees (0 when achromatic).</summary>
    private static (double Max, double Min, double Hue) GetRangeAndHue(Rgb rgb)
    {
        var r = rgb.R / 255.0;
        var g = rgb.G / 255.0;
        var b = rgb.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        if (delta == 0)
            return (max, min, 0);

        var sector = max == r
            ? (g - b) / delta % 6
            : max == g
                ? (b - r) / delta + 2
                : (r - g) / delta + 4;

        return (max, min, NormalizeHue(sector * 60));
    }

    #endregion

    #region OKLCH

    public static Oklch RgbToOklch(Rgb rgb)
    {
        var r = SrgbToLinear(rgb.R);
        var g = SrgbToLinear(rgb.G);
        var b = SrgbToLinear(rgb.B);

        var longCone = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var mediumCone = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var shortCone = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);

        var lightness = 0.2104542553 * longCone + 0.793617785 * mediumCone - 0.0040720468 * shortCone;
        var axisA = 1.9779984951 * longCone - 2.428592205 * mediumCone + 0.4505937099 * shortCone;
        var axisB = 0.0259040371 * longCone + 0.7827717662 * mediumCone - 0.808675766 * shortCone;
        var chroma = Math.Sqrt(axisA * axisA + axisB * axisB);

        // Hue is meaningless for grays; keep it stable instead of exposing numeric noise.
        var hue = chroma < AchromaticChromaThreshold
            ? 0
            : NormalizeHue(Math.Atan2(axisB, axisA) * 180 / Math.PI);

        return new Oklch(lightness, chroma, hue);
    }

    public static Rgb OklchToRgb(double lightness, double chroma, double hue)
    {
        var (r, g, b) = OklchToLinearRgb(lightness, chroma, hue);

        return new Rgb(
            ToByte(LinearToSrgb(r)),
            ToByte(LinearToSrgb(g)),
            ToByte(LinearToSrgb(b)));
    }

    /// <summary>Highest chroma (3 decimals, rounded down) that stays inside the sRGB gamut for the given lightness/hue.</summary>
    public static double GetMaxOklchChroma(double lightness, double hue)
    {
        const double upperBound = 0.5;
        const int iterations = 12;

        if (IsInSrgbGamut(lightness, upperBound, hue))
            return upperBound;

        // For a fixed lightness/hue the in-gamut chroma range is [0, max], so bisection applies.
        double low = 0, high = upperBound;
        for (var i = 0; i < iterations; i++)
        {
            var mid = (low + high) / 2;
            if (IsInSrgbGamut(lightness, mid, hue))
                low = mid;
            else
                high = mid;
        }

        return Math.Max(0.001, Math.Floor(low * 1000) / 1000);
    }

    private static bool IsInSrgbGamut(double lightness, double chroma, double hue)
    {
        const double epsilon = 0.001;
        var (r, g, b) = OklchToLinearRgb(lightness, chroma, hue);

        return IsEncodedInRange(r) && IsEncodedInRange(g) && IsEncodedInRange(b);

        static bool IsEncodedInRange(double linear)
        {
            var encoded = LinearToSrgb(linear);
            return encoded is >= -epsilon and <= 1 + epsilon;
        }
    }

    private static (double R, double G, double B) OklchToLinearRgb(double lightness, double chroma, double hue)
    {
        var hueRadians = hue * Math.PI / 180;
        var axisA = chroma * Math.Cos(hueRadians);
        var axisB = chroma * Math.Sin(hueRadians);

        var longCone = Cube(lightness + 0.3963377774 * axisA + 0.2158037573 * axisB);
        var mediumCone = Cube(lightness - 0.1055613458 * axisA - 0.0638541728 * axisB);
        var shortCone = Cube(lightness - 0.0894841775 * axisA - 1.291485548 * axisB);

        return (
            4.0767416621 * longCone - 3.3077115913 * mediumCone + 0.2309699292 * shortCone,
            -1.2684380046 * longCone + 2.6097574011 * mediumCone - 0.3413193965 * shortCone,
            -0.0041960863 * longCone - 0.7034186147 * mediumCone + 1.707614701 * shortCone);

        static double Cube(double x) => x * x * x;
    }

    private static double SrgbToLinear(int channel)
    {
        var normalized = channel / 255.0;
        return normalized <= 0.04045
            ? normalized / 12.92
            : Math.Pow((normalized + 0.055) / 1.055, 2.4);
    }

    private static double LinearToSrgb(double channel)
    {
        var absolute = Math.Abs(channel);
        return absolute > 0.0031308
            ? Math.Sign(channel) * (1.055 * Math.Pow(absolute, 1 / 2.4) - 0.055)
            : 12.92 * channel;
    }

    #endregion

    #region Hex parsing / formatting

    /// <summary>Parses 3-, 4-, 6- or 8-digit hex, with or without a single leading <c>#</c>.</summary>
    public static ParsedHexColor? ParseHexColor(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var raw = input.Trim();
        if (raw.StartsWith('#'))
            raw = raw[1..];

        if (raw.Length is 3 or 4)
            raw = string.Concat(raw.Select(character => new string(character, 2)));

        if (raw.Length is not (6 or 8) || !raw.All(char.IsAsciiHexDigit))
            return null;

        var bytes = Convert.FromHexString(raw);
        var rgb = new Rgb(bytes[0], bytes[1], bytes[2]);
        int? alphaPercent = bytes.Length == 4
            ? (int)Math.Round(bytes[3] / 255.0 * 100, Rounding)
            : null;

        return new ParsedHexColor(rgb, alphaPercent);
    }

    /// <summary>Normalized <c>#RRGGBB[AA]</c> form of a hex input, or null when unparsable.</summary>
    public static string? NormalizeColorValue(string? color, int alphaFallback = 100)
    {
        var parsed = ParseHexColor(color);
        return parsed is null
            ? null
            : FormatHex(parsed.Value.Rgb, parsed.Value.AlphaPercent ?? alphaFallback);
    }

    public static string ToHexByte(int channel) =>
        Math.Clamp(channel, 0, 255).ToString("X2", CultureInfo.InvariantCulture);

    /// <summary><c>#RRGGBB</c>, or <c>#RRGGBBAA</c> when alpha is below 100.</summary>
    public static string FormatHex(Rgb rgb, int alpha) =>
        "#" + GetDisplayHex(rgb, alpha, showAlpha: true);

    /// <summary>Hex without the leading <c>#</c>; 8 digits when alpha is shown and below 100.</summary>
    public static string GetDisplayHex(Rgb rgb, int alpha, bool showAlpha)
    {
        var hex = $"{ToHexByte(rgb.R)}{ToHexByte(rgb.G)}{ToHexByte(rgb.B)}";
        return showAlpha && alpha < 100
            ? hex + ToHexByte(ToByte(Math.Clamp(alpha, 0, 100) / 100.0))
            : hex;
    }

    public static string FormatRgb(Rgb rgb, int alpha) =>
        alpha >= 100
            ? $"rgb({rgb.R}, {rgb.G}, {rgb.B})"
            : $"rgba({rgb.R}, {rgb.G}, {rgb.B}, {FormatAlpha(alpha)})";

    public static string FormatHsl(Rgb rgb, int alpha)
    {
        var hsl = RgbToHsl(rgb);
        return alpha >= 100
            ? $"hsl({hsl.H}, {hsl.S}%, {hsl.L}%)"
            : $"hsla({hsl.H}, {hsl.S}%, {hsl.L}%, {FormatAlpha(alpha)})";
    }

    public static string FormatOklch(Rgb rgb, int alpha)
    {
        var oklch = RgbToOklch(rgb);
        var lightness = oklch.L.ToString("0.###", CultureInfo.InvariantCulture);
        var chroma = oklch.C.ToString("0.###", CultureInfo.InvariantCulture);
        var hue = (Math.Round(oklch.H, Rounding) % 360).ToString("0", CultureInfo.InvariantCulture);

        return alpha >= 100
            ? $"oklch({lightness} {chroma} {hue})"
            : $"oklch({lightness} {chroma} {hue} / {FormatAlpha(alpha)})";
    }

    private static string FormatAlpha(int alpha) =>
        (Math.Clamp(alpha, 0, 100) / 100.0).ToString("0.##", CultureInfo.InvariantCulture);

    #endregion

    #region Helpers

    public static int ClampPercent(double value) =>
        (int)Math.Clamp(Math.Round(value, Rounding), 0, 100);

    public static double NormalizeHue(double hue)
    {
        var normalized = hue % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }

    /// <summary>Converts a 0–1 channel to a clamped 0–255 byte.</summary>
    private static int ToByte(double unit) =>
        (int)Math.Clamp(Math.Round(unit * 255, Rounding), 0, 255);

    #endregion
}