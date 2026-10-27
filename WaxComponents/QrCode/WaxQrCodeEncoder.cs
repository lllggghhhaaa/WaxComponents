namespace WaxComponents.QrCode;

/// <summary>Result of encoding text into a QR Code symbol.</summary>
/// <param name="IsFinder">Modules belonging to the three 7x7 finder patterns; the component draws these as styled eyes.</param>
/// <param name="IsFunction">All structural modules (finders, separators, timing, alignment, format, version, dark module); the logo excavation must keep these.</param>
internal sealed record QrMatrix(int Size, bool[,] Modules, bool[,] IsFinder, bool[,] IsFunction);

/// <summary>
/// Pure C# QR Code encoder (ISO/IEC 18004) for WaxQrCode: byte mode (UTF-8), versions 1–40,
/// all four error-correction levels, Reed-Solomon ECC, interleaved blocks and automatic mask
/// selection with the four spec penalties. Draws no styling — the component renders the matrix
/// and the three finder-pattern regions (<see cref="QrMatrix.IsFinder"/>) as SVG.
/// </summary>
internal static class WaxQrCodeEncoder
{
    /// <summary>Byte-mode data capacity in characters per (version-1, EC level) — spec tables derived at runtime.</summary>
    private static readonly int[] TotalCodewordsPerVersion =
    [
        26, 44, 70, 100, 134, 172, 196, 242, 292, 346,
        404, 466, 532, 581, 655, 733, 815, 901, 991, 1085,
        1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185,
        2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706,
    ];

    // ECC codewords per block, indexed [ecLevel * 40 + (version - 1)] for levels L, M, Q, H.
    private static readonly byte[] EccCodewordsPerBlock =
    [
        // L
        7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28,
        28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30,
        // M
        10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26,
        26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
        // Q
        13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30,
        28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30,
        // H
        17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28,
        30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30,
    ];

    // Number of error-correction blocks, indexed the same way.
    private static readonly byte[] EccBlockCounts =
    [
        // L
        1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8,
        8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25,
        // M
        1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16,
        17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49,
        // Q
        1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20,
        23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68,
        // H
        1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25,
        25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81,
    ];

    // Alignment pattern center coordinates per version-1 (same for all EC levels).
    private static readonly byte[][] AlignmentPositions =
    [
        [],
        [6, 18],
        [6, 22],
        [6, 26],
        [6, 30],
        [6, 34],
        [6, 22, 38],
        [6, 24, 42],
        [6, 26, 46],
        [6, 28, 50],
        [6, 30, 54],
        [6, 32, 58],
        [6, 34, 62],
        [6, 26, 46, 66],
        [6, 26, 48, 70],
        [6, 26, 50, 74],
        [6, 30, 54, 78],
        [6, 30, 56, 82],
        [6, 30, 58, 86],
        [6, 34, 62, 90],
        [6, 28, 50, 72, 94],
        [6, 26, 50, 74, 98],
        [6, 30, 54, 78, 102],
        [6, 28, 54, 80, 106],
        [6, 32, 58, 84, 110],
        [6, 30, 58, 86, 114],
        [6, 34, 62, 90, 118],
        [6, 26, 50, 74, 98, 122],
        [6, 30, 54, 78, 102, 126],
        [6, 26, 52, 78, 104, 130],
        [6, 30, 56, 82, 108, 134],
        [6, 34, 60, 86, 112, 138],
        [6, 30, 58, 86, 114, 142],
        [6, 34, 62, 90, 118, 146],
        [6, 30, 54, 78, 102, 126, 150],
        [6, 24, 50, 76, 102, 128, 154],
        [6, 28, 54, 80, 106, 132, 158],
        [6, 32, 58, 84, 110, 136, 162],
        [6, 26, 54, 82, 110, 138, 166],
        [6, 30, 58, 86, 114, 142, 170],
    ];

    // Galois field GF(2^8) tables for Reed-Solomon, modulus polynomial x^8 + x^4 + x^3 + x^2 + 1.
    private static readonly int[] GfExp = new int[512];
    private static readonly int[] GfLog = new int[256];

    static WaxQrCodeEncoder()
    {
        for (int i = 0, x = 1; i < 255; i++)
        {
            GfExp[i] = x;
            GfLog[x] = i;
            x <<= 1;
            if (x >= 256) x ^= 0x11D;
        }

        for (int i = 255; i < 512; i++)
        {
            GfExp[i] = GfExp[i - 255];
        }
    }

    private static int GfMul(int x, int y) => x == 0 || y == 0 ? 0 : GfExp[GfLog[x] + GfLog[y]];

    /// <summary>Encodes UTF-8 text; <paramref name="forcedVersion"/> (1–40) and <paramref name="forcedMask"/> (0–7) are for tests.</summary>
    public static QrMatrix Encode(string text, WaxQrCodeErrorCorrection ec, int? forcedVersion = null, int? forcedMask = null)
    {
        int ecIndex = (int)ec;
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);

        int version = forcedVersion ?? ChooseVersion(data.Length, ecIndex);
        int size = version * 4 + 17;

        var codewords = AddEccAndInterleave(BuildCodewords(data, version, ecIndex), version, ecIndex);

        bool[,] modules = new bool[size, size];
        bool[,] isFunction = new bool[size, size];
        DrawFunctionPatterns(modules, isFunction, version);

        bool[,] isFinder = ExtractFinderMask(isFunction);

        var masked = ChooseMaskAndPlace(modules, isFunction, codewords, version, ecIndex, forcedMask);

        // Format info depends on the chosen mask; the placeholder copy is overwritten in place.
        DrawFormatBits(masked.modules, isFunction, ecIndex, masked.mask);

        return new QrMatrix(size, masked.modules, isFinder, isFunction);
    }

    // Version / codeword sizing ------------------------------------------------------------------

    private static int GetRawDataModules(int version)
    {
        int result = (16 * version + 128) * version + 64;
        if (version >= 2)
        {
            int numAlign = version / 7 + 2;
            result -= (25 * numAlign - 10) * numAlign - 55;
            if (version >= 7)
            {
                result -= 36;
            }
        }

        return result;
    }

    private static int GetDataCodewords(int version, int ecIndex) =>
        GetRawDataModules(version) / 8 - EccCodewordsPerBlock[ecIndex * 40 + version - 1] * EccBlockCounts[ecIndex * 40 + version - 1];

    private static int ChooseVersion(int dataLength, int ecIndex)
    {
        for (int version = 1; version <= 40; version++)
        {
            int capacityBits = GetDataCodewords(version, ecIndex) * 8;
            int countBits = version < 10 ? 8 : 16;
            if (dataLength * 8 <= capacityBits - 4 - countBits)
            {
                return version;
            }
        }

        throw new ArgumentException("Text is too long to encode in a QR code (byte mode, 4095 byte limit).");
    }

    private static byte[] BuildCodewords(byte[] data, int version, int ecIndex)
    {
        int capacityBits = GetDataCodewords(version, ecIndex) * 8;
        var bits = new List<bool>(capacityBits);

        void Append(int value, int length)
        {
            for (int i = length - 1; i >= 0; i--)
            {
                bits.Add((value >> i & 1) != 0);
            }
        }

        Append(4, 4); // byte mode
        Append(data.Length, version < 10 ? 8 : 16);
        foreach (var b in data)
        {
            Append(b, 8);
        }

        if (bits.Count > capacityBits)
        {
            throw new ArgumentException("Text is too long to encode in a QR code.");
        }

        Append(0, Math.Min(4, capacityBits - bits.Count)); // terminator
        Append(0, (8 - bits.Count % 8) % 8);               // pad to byte boundary

        for (int pad = 0xEC; bits.Count < capacityBits; pad ^= 0xEC ^ 0x11)
        {
            Append(pad, 8);
        }

        var codewords = new byte[bits.Count / 8];
        for (int i = 0; i < codewords.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (bits[i * 8 + j])
                {
                    codewords[i] |= (byte)(1 << (7 - j));
                }
            }
        }

        return codewords;
    }

    // Reed-Solomon + block interleaving -----------------------------------------------------------

    private static byte[] ReedSolomonRemainder(ReadOnlySpan<byte> data, byte[] divisor)
    {
        // Synthetic division; divisor includes the leading 1, so index i + 1 aligns coefficients.
        var result = new byte[divisor.Length - 1];
        foreach (var b in data)
        {
            int factor = b ^ result[0];
            result.AsSpan(1).CopyTo(result);
            result[^1] = 0;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ^= (byte)GfMul(divisor[i + 1], factor);
            }
        }

        return result;
    }

    private static byte[] AddEccAndInterleave(byte[] data, int version, int ecIndex)
    {
        int tableIndex = ecIndex * 40 + version - 1;
        int numBlocks = EccBlockCounts[tableIndex];
        int blockEccLen = EccCodewordsPerBlock[tableIndex];
        int rawCodewords = GetRawDataModules(version) / 8;
        int numShortBlocks = numBlocks - rawCodewords % numBlocks;
        int shortBlockLen = rawCodewords / numBlocks;

        byte[] divisor = [1];
        for (int i = 0; i < blockEccLen; i++)
        {
            var next = new byte[divisor.Length + 1];
            divisor.CopyTo(next, 0);
            for (int j = 0; j < divisor.Length; j++)
            {
                next[j + 1] ^= (byte)GfMul(divisor[j], GfExp[i]);
            }

            divisor = next;
        }

        var blocks = new byte[numBlocks][];
        for (int i = 0, k = 0; i < numBlocks; i++)
        {
            int dataLen = shortBlockLen - blockEccLen + 1 - (i < numShortBlocks ? 1 : 0);
            var blockData = data.AsSpan(k, dataLen).ToArray();
            k += dataLen;
            var ecc = ReedSolomonRemainder(blockData, divisor);

            // Short blocks get a phantom pad byte so every block has length shortBlockLen + 1;
            // the interleave step skips that position for short blocks.
            var full = new byte[shortBlockLen + 1];
            blockData.CopyTo(full, 0);
            if (i < numShortBlocks)
            {
                full[dataLen] = 0;
            }

            ecc.CopyTo(full, full.Length - blockEccLen);
            blocks[i] = full;
        }

        var result = new byte[rawCodewords];
        for (int i = 0, k = 0; i < blocks[0].Length; i++)
        {
            for (int j = 0; j < numBlocks; j++)
            {
                if (i != shortBlockLen - blockEccLen || j >= numShortBlocks)
                {
                    result[k++] = blocks[j][i];
                }
            }
        }

        return result;
    }

    // Function patterns ---------------------------------------------------------------------------

    private static void SetFunctionModule(bool[,] modules, bool[,] isFunction, int x, int y, bool dark)
    {
        modules[y, x] = dark;
        isFunction[y, x] = true;
    }

    private static void DrawFunctionPatterns(bool[,] modules, bool[,] isFunction, int version)
    {
        int size = modules.GetLength(0);

        // Timing patterns.
        for (int i = 8; i < size - 8; i++)
        {
            SetFunctionModule(modules, isFunction, i, 6, i % 2 == 0);
            SetFunctionModule(modules, isFunction, 6, i, i % 2 == 0);
        }

        // Alignment patterns, skipping the three finder corners.
        var positions = AlignmentPositions[version - 1];
        int count = positions.Length;
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if ((i == 0 && j == 0) || (i == 0 && j == count - 1) || (i == count - 1 && j == 0))
                {
                    continue;
                }

                DrawDensePattern(modules, isFunction, positions[i], positions[j], 2);
            }
        }

        // Placeholder format info (redrawn with the chosen mask) and the dark module.
        DrawFormatBits(modules, isFunction, 0, 0);

        // Version information.
        if (version >= 7)
        {
            int rem = version;
            for (int i = 0; i < 12; i++)
            {
                rem = rem << 1 ^ ((rem >> 11) * 0x1F25);
            }

            int bits = version << 12 | rem;
            for (int i = 0; i < 18; i++)
            {
                bool bit = (bits >> i & 1) != 0;
                int a = size - 11 + i % 3;
                int b = i / 3;
                SetFunctionModule(modules, isFunction, a, b, bit);
                SetFunctionModule(modules, isFunction, b, a, bit);
            }
        }

        // Finder patterns last so they win over anything touching them.
        DrawFinderPattern(modules, isFunction, 3, 3);
        DrawFinderPattern(modules, isFunction, size - 4, 3);
        DrawFinderPattern(modules, isFunction, 3, size - 4);
    }

    private static void DrawDensePattern(bool[,] modules, bool[,] isFunction, int cx, int cy, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                SetFunctionModule(modules, isFunction, cx + dx, cy + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
            }
        }
    }

    private static void DrawFinderPattern(bool[,] modules, bool[,] isFunction, int cx, int cy)
    {
        // Includes the one-module light separator (distance 4) around the 7x7 ring.
        for (int dy = -4; dy <= 4; dy++)
        {
            for (int dx = -4; dx <= 4; dx++)
            {
                int x = cx + dx;
                int y = cy + dy;
                if (x >= 0 && x < modules.GetLength(0) && y >= 0 && y < modules.GetLength(0))
                {
                    SetFunctionModule(modules, isFunction, x, y, Math.Max(Math.Abs(dx), Math.Abs(dy)) is not (2 or 4));
                }
            }
        }
    }

    private static void DrawFormatBits(bool[,] modules, bool[,] isFunction, int ecIndex, int mask)
    {
        int size = modules.GetLength(0);
        int eccBits = ecIndex switch
        {
            0 => 1, // L
            1 => 0, // M
            2 => 3, // Q
            3 => 2, // H
            _ => throw new ArgumentOutOfRangeException(nameof(ecIndex)),
        };

        int data = eccBits << 3 | mask;
        int rem = data;
        for (int i = 0; i < 10; i++)
        {
            rem = rem << 1 ^ ((rem >> 9) * 0x537);
        }

        int bits = (data << 10 | rem) ^ 0x5412;

        void Set(int x, int y, bool dark) => SetFunctionModule(modules, isFunction, x, y, dark);

        // First copy: around the top-left finder.
        for (int i = 0; i <= 5; i++)
        {
            Set(8, i, (bits >> i & 1) != 0);
        }

        Set(8, 7, (bits >> 6 & 1) != 0);
        Set(8, 8, (bits >> 7 & 1) != 0);
        Set(7, 8, (bits >> 8 & 1) != 0);
        for (int i = 9; i < 15; i++)
        {
            Set(14 - i, 8, (bits >> i & 1) != 0);
        }

        // Second copy: split across the other two finders.
        for (int i = 0; i < 8; i++)
        {
            Set(size - 1 - i, 8, (bits >> i & 1) != 0);
        }

        for (int i = 8; i < 15; i++)
        {
            Set(8, size - 15 + i, (bits >> i & 1) != 0);
        }

        Set(8, size - 8, true); // always-dark module
    }

    private static bool[,] ExtractFinderMask(bool[,] isFunction)
    {
        int size = isFunction.GetLength(0);
        var finder = new bool[size, size];
        foreach (var (cx, cy) in new[] { (3, 3), (size - 4, 3), (3, size - 4) })
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                for (int dx = -3; dx <= 3; dx++)
                {
                    finder[cy + dy, cx + dx] = true;
                }
            }
        }

        return finder;
    }

    // Data placement, masking and penalty scoring -------------------------------------------------

    private static (bool[,] modules, int mask) ChooseMaskAndPlace(
        bool[,] modules, bool[,] isFunction, byte[] codewords, int version, int ecIndex, int? forcedMask)
    {
        int size = modules.GetLength(0);

        int mask = forcedMask ?? 0;
        if (forcedMask is null)
        {
            int bestPenalty = int.MaxValue;
            for (int candidate = 0; candidate < 8; candidate++)
            {
                var candidateModules = (bool[,])modules.Clone();
                PlaceData(candidateModules, isFunction, codewords);
                ApplyMask(candidateModules, isFunction, candidate);
                DrawFormatBits(candidateModules, isFunction, ecIndex, candidate);

                int penalty = PenaltyScore(candidateModules);
                if (penalty < bestPenalty)
                {
                    bestPenalty = penalty;
                    mask = candidate;
                }
            }
        }

        PlaceData(modules, isFunction, codewords);
        ApplyMask(modules, isFunction, mask);
        return (modules, mask);
    }

    private static void PlaceData(bool[,] modules, bool[,] isFunction, byte[] codewords)
    {
        int size = modules.GetLength(0);
        int bitIndex = 0;

        for (int right = size - 1; right >= 1; right -= 2)
        {
            if (right == 6)
            {
                right = 5;
            }

            for (int vert = 0; vert < size; vert++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int x = right - j;
                    bool upward = (right + 1 & 2) == 0;
                    int y = upward ? size - 1 - vert : vert;

                    if (!isFunction[y, x] && bitIndex < codewords.Length * 8)
                    {
                        modules[y, x] = (codewords[bitIndex >> 3] >> (7 - (bitIndex & 7)) & 1) != 0;
                        bitIndex++;
                    }
                }
            }
        }
    }

    private static void ApplyMask(bool[,] modules, bool[,] isFunction, int mask)
    {
        int size = modules.GetLength(0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (isFunction[y, x])
                {
                    continue;
                }

                bool invert = mask switch
                {
                    0 => (x + y) % 2 == 0,
                    1 => y % 2 == 0,
                    2 => x % 3 == 0,
                    3 => (x + y) % 3 == 0,
                    4 => (x / 3 + y / 2) % 2 == 0,
                    5 => x * y % 2 + x * y % 3 == 0,
                    6 => (x * y % 2 + x * y % 3) % 2 == 0,
                    7 => ((x + y) % 2 + x * y % 3) % 2 == 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(mask)),
                };

                if (invert)
                {
                    modules[y, x] = !modules[y, x];
                }
            }
        }
    }

    private static int PenaltyScore(bool[,] modules)
    {
        int size = modules.GetLength(0);
        int penalty = 0;

        for (int y = 0; y < size; y++)
        {
            penalty += LinePenalty(x => modules[y, x], size);
        }

        for (int x = 0; x < size; x++)
        {
            penalty += LinePenalty(y => modules[y, x], size);
        }

        // 2x2 blocks of the same color.
        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
                bool color = modules[y, x];
                if (color == modules[y, x + 1] && color == modules[y + 1, x] && color == modules[y + 1, x + 1])
                {
                    penalty += 3;
                }
            }
        }

        // Dark-module proportion: 10 points per 5% deviation from 50%.
        int dark = 0;
        foreach (var m in modules)
        {
            if (m)
            {
                dark++;
            }
        }

        int percent = dark * 100 / (size * size);
        penalty += Math.Abs(percent - 50) / 5 * 10;

        return penalty;
    }

    /// <summary>N1 (runs of ≥5 same-color cells) and N3 (finder-like 1:1:3:1:1 runs with four light modules on either side, 40 points each) for one row or column.</summary>
    private static int LinePenalty(Func<int, bool> cell, int size)
    {
        int penalty = 0;

        int runStart = 0;
        for (int i = 1; i <= size; i++)
        {
            if (i == size || cell(i) != cell(runStart))
            {
                int runLength = i - runStart;
                if (runLength >= 5)
                {
                    penalty += 3 + runLength - 5;
                }

                runStart = i;
            }
        }

        for (int p = 0; p <= size - 7; p++)
        {
            bool pattern =
                cell(p) && !cell(p + 1) && cell(p + 2) && cell(p + 3) && cell(p + 4) && !cell(p + 5) && cell(p + 6);
            if (!pattern)
            {
                continue;
            }

            bool lightBefore = true;
            for (int i = Math.Max(0, p - 4); i < p; i++)
            {
                lightBefore &= !cell(i);
            }

            bool lightAfter = true;
            for (int i = p + 7; i < Math.Min(size, p + 11); i++)
            {
                lightAfter &= !cell(i);
            }

            if (lightBefore || lightAfter)
            {
                penalty += 40;
            }
        }

        return penalty;
    }
}
