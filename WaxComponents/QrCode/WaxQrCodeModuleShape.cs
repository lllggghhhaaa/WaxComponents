namespace WaxComponents.QrCode;

/// <summary>Corner style of the dark modules. Finder eyes are styled separately (see <see cref="WaxQrCodeEyeShape"/>).</summary>
public enum WaxQrCodeModuleShape
{
    /// <summary>Classic square modules; the most compact and conventional look.</summary>
    Square,

    /// <summary>Modules with rounded outer corners that blend into connected blobs, matching the library's soft look.</summary>
    Rounded,

    /// <summary>Isolated circles; the airiest look, slightly harder to scan at very small sizes.</summary>
    Dot,
}
