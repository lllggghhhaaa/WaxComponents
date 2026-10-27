namespace WaxComponents.QrCode;

/// <summary>QR error-correction level; higher levels restore more damaged or covered modules but store less data.</summary>
public enum WaxQrCodeErrorCorrection
{
    /// <summary>~7% recovery.</summary>
    Low,

    /// <summary>~15% recovery. Sensible default and the minimum recommended when a logo is shown.</summary>
    Medium,

    /// <summary>~25% recovery.</summary>
    Quartile,

    /// <summary>~30% recovery. Recommended with a large logo.</summary>
    High,
}
