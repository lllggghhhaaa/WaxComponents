using System.Globalization;
using System.Text;

namespace WaxComponents.QrCode;

/// <summary>SVG path/geometry strings produced by <see cref="WaxQrCodeRenderer"/> for one WaxQrCode render.</summary>
/// <param name="ModulePath">Dark non-finder modules in module coordinates (data + timing/alignment/format/version/dark-module patterns).</param>
/// <param name="EyePath">The three finder eyes (outer ring hole + center square), to be drawn with fill-rule evenodd.</param>
/// <param name="LogoX">Logo box left edge in module coordinates; meaningful only when <see cref="HasLogo"/>.</param>
/// <param name="LogoY">Logo box top edge in module coordinates.</param>
/// <param name="LogoSize">Logo box edge length in module units.</param>
/// <param name="HasLogo">Whether a logo box was requested (drives excavation and the image element).</param>
internal sealed record WaxQrCodeGeometry(string ModulePath, string EyePath, double LogoX, double LogoY, double LogoSize, bool HasLogo);

/// <summary>
/// Turns a <see cref="QrMatrix"/> into SVG path data for WaxQrCode. Everything is drawn in module
/// coordinates (one unit per module); the component offsets by the quiet zone with a group
/// transform. Finder patterns are drawn separately as eyes so the modules can take any shape
/// without weakening the most scan-critical part of the symbol. A centered logo box excavates the
/// data modules underneath it (structural patterns stay put) so the logo floats on clear ground.
/// </summary>
internal static class WaxQrCodeRenderer
{
    // Clear margin kept around the logo box, in module units. Kept at half a module so a 20%
    // logo stays within Medium error correction's budget: at Level M a version-2 symbol
    // corrects 8 codewords, and each cleared module row costs roughly one codeword.
    private const double LogoClearance = 0.5;

    public static WaxQrCodeGeometry Render(
        QrMatrix matrix,
        WaxQrCodeModuleShape moduleShape,
        WaxQrCodeEyeShape eyeShape,
        int? logoSizePercent)
    {
        int size = matrix.Size;
        bool[,] modules = matrix.Modules;
        bool[,] isFunction = matrix.IsFunction;
        bool isFinder(int x, int y) => matrix.IsFinder[y, x];

        double? logoHalf = null;
        double logoSize = 0;
        if (logoSizePercent is { } percent)
        {
            logoSize = size * percent / 100.0;
            logoHalf = logoSize / 2 + LogoClearance;
        }

        double center = size / 2.0;

        bool Excavated(int x, int y)
        {
            // Structural patterns (timing, alignment, format, version, dark module) are never
            // removed: a broken structure pattern is unrecoverable, while data loss is what the
            // error correction level is for.
            if (logoHalf is null || isFunction[y, x])
            {
                return false;
            }

            return Math.Abs(x + 0.5 - center) <= logoHalf && Math.Abs(y + 0.5 - center) <= logoHalf;
        }

        bool Dark(int x, int y) => x >= 0 && x < size && y >= 0 && y < size && modules[y, x];

        var modulePath = new StringBuilder();

        if (moduleShape == WaxQrCodeModuleShape.Square)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!modules[y, x] || isFinder(x, y) || Excavated(x, y))
                    {
                        continue;
                    }

                    // Collapse horizontal runs of modules into a single rect.
                    int runStart = x;
                    while (x + 1 < size && modules[y, x + 1] && !isFinder(x + 1, y) && !Excavated(x + 1, y))
                    {
                        x++;
                    }

                    AppendSquareRun(modulePath, runStart, y, x - runStart + 1);
                }
            }
        }
        else
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!modules[y, x] || isFinder(x, y) || Excavated(x, y))
                    {
                        continue;
                    }

                    if (moduleShape == WaxQrCodeModuleShape.Dot)
                    {
                        AppendDot(modulePath, x, y);
                    }
                    else
                    {
                        AppendRoundedModule(modulePath, x, y, Dark);
                    }
                }
            }
        }

        // Eyes: ring (7x7 outer + 5x5 hole) and 3x3 center, resolved by fill-rule evenodd.
        var eyePath = new StringBuilder();
        double radiusOuter = eyeShape == WaxQrCodeEyeShape.Rounded ? 2 : 0;
        double radiusHole = eyeShape == WaxQrCodeEyeShape.Rounded ? 1.5 : 0;
        double radiusCenter = eyeShape == WaxQrCodeEyeShape.Rounded ? 1 : 0;
        foreach (var (cx, cy) in new[] { (3, 3), (size - 4, 3), (3, size - 4) })
        {
            AppendRect(eyePath, cx - 3, cy - 3, 7, 7, radiusOuter);
            AppendRect(eyePath, cx - 2, cy - 2, 5, 5, radiusHole);
            AppendRect(eyePath, cx - 1, cy - 1, 3, 3, radiusCenter);
        }

        return new WaxQrCodeGeometry(
            modulePath.ToString(),
            eyePath.ToString(),
            (size - logoSize) / 2,
            (size - logoSize) / 2,
            logoSize,
            logoHalf is not null);
    }

    private static string F(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static void AppendSquareRun(StringBuilder path, int x, int y, int width)
    {
        path.Append('M').Append(F(x)).Append(' ').Append(F(y))
            .Append('h').Append(F(width))
            .Append("v1h-").Append(F(width))
            .Append("z");
    }

    private static void AppendDot(StringBuilder path, int x, int y)
    {
        path.Append('M').Append(F(x + 0.5)).Append(' ').Append(F(y))
            .Append("a.5.5 0 1 0 0 1")
            .Append("a.5.5 0 1 0 0 -1")
            .Append('z');
    }

    private static void AppendRoundedModule(StringBuilder path, int x, int y, Func<int, int, bool> dark)
    {
        // A corner is rounded only when both of its orthogonal neighbours are light, so adjacent
        // modules merge into smooth blobs while inner corners stay sharp.
        double rtl = !dark(x - 1, y) && !dark(x, y - 1) ? 0.5 : 0;
        double rtr = !dark(x + 1, y) && !dark(x, y - 1) ? 0.5 : 0;
        double rbr = !dark(x + 1, y) && !dark(x, y + 1) ? 0.5 : 0;
        double rbl = !dark(x - 1, y) && !dark(x, y + 1) ? 0.5 : 0;

        path.Append('M').Append(F(x + rtl)).Append(' ').Append(F(y));
        path.Append('H').Append(F(x + 1 - rtr));
        if (rtr > 0) path.Append('A').Append(F(rtr)).Append(' ').Append(F(rtr)).Append(" 0 0 1 ").Append(F(x + 1)).Append(' ').Append(F(y + rtr));
        path.Append('V').Append(F(y + 1 - rbr));
        if (rbr > 0) path.Append('A').Append(F(rbr)).Append(' ').Append(F(rbr)).Append(" 0 0 1 ").Append(F(x + 1 - rbr)).Append(' ').Append(F(y + 1));
        path.Append('H').Append(F(x + rbl));
        if (rbl > 0) path.Append('A').Append(F(rbl)).Append(' ').Append(F(rbl)).Append(" 0 0 1 ").Append(F(x)).Append(' ').Append(F(y + 1 - rbl));
        path.Append('V').Append(F(y + rtl));
        if (rtl > 0) path.Append('A').Append(F(rtl)).Append(' ').Append(F(rtl)).Append(" 0 0 1 ").Append(F(x + rtl)).Append(' ').Append(F(y));
        path.Append('Z');
    }

    private static void AppendRect(StringBuilder path, double x, double y, double width, double height, double radius)
    {
        if (radius <= 0)
        {
            path.Append('M').Append(F(x)).Append(' ').Append(F(y))
                .Append('H').Append(F(x + width))
                .Append('V').Append(F(y + height))
                .Append('H').Append(F(x))
                .Append('Z');
            return;
        }

        path.Append('M').Append(F(x + radius)).Append(' ').Append(F(y))
            .Append('H').Append(F(x + width - radius))
            .Append('A').Append(F(radius)).Append(' ').Append(F(radius)).Append(" 0 0 1 ").Append(F(x + width)).Append(' ').Append(F(y + radius))
            .Append('V').Append(F(y + height - radius))
            .Append('A').Append(F(radius)).Append(' ').Append(F(radius)).Append(" 0 0 1 ").Append(F(x + width - radius)).Append(' ').Append(F(y + height))
            .Append('H').Append(F(x + radius))
            .Append('A').Append(F(radius)).Append(' ').Append(F(radius)).Append(" 0 0 1 ").Append(F(x)).Append(' ').Append(F(y + height - radius))
            .Append('V').Append(F(y + radius))
            .Append('A').Append(F(radius)).Append(' ').Append(F(radius)).Append(" 0 0 1 ").Append(F(x + radius)).Append(' ').Append(F(y))
            .Append('Z');
    }
}
