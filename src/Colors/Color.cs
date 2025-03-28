using System;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace ShapeCrawler;
#pragma warning restore IDE0130

/// <summary>
///     Color.
/// </summary>
public struct Color
{
    /// <summary>
    ///     Predefined black color.
    /// </summary>
    public static readonly Color Black = new(0, 0, 0);

    /// <summary>
    ///     Predefined transparent color.
    /// </summary>
    public static readonly Color NoColor = new(0, 0, 0, 0);

    /// <summary>
    ///     Predefined white color.
    /// </summary>
    public static readonly Color White = new(255, 255, 255);

    /// <summary>
    ///     Max opacity value, equivalent to 1.
    /// </summary>
    internal const float Opacity = 255;

    private readonly int blue;
    private readonly int green;
    private readonly int red;

    private Color(int red, int green, int blue)
        : this(red, green, blue, 255)
    {
    }

    private Color(int red, int green, int blue, float alpha)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
        this.Alpha = alpha;
    }

    /// <summary>
    ///     Gets or sets the alpha value.
    /// </summary>
    /// <remarks>
    /// Values are 0 to 255, where 0 is totally transparent.
    /// </remarks>
    public float Alpha { get; set; }

    /// <summary>
    ///     Gets hexadecimal code.
    /// </summary>
    public string Hex => this.ToString();

    /// <summary>
    ///     Gets a value indicating whether the color is solid.
    /// </summary>
    internal readonly bool IsSolid => Math.Abs(this.Alpha - 255) < 0.01;

    /// <summary>
    ///     Gets a value indicating whether the color is transparent.
    /// </summary>
    internal readonly bool IsTransparent => Math.Abs(this.Alpha) < 0.01;

    /// <summary>
    ///     Creates color from Hex value.
    /// </summary>
    /// <param name="hex">Hex value.</param>
    /// <returns>Returns <see langword="true" /> if hex is a valid value. </returns>
    public static Color FromHex(string hex)
    {
        var value = hex.StartsWith("#", StringComparison.Ordinal) ? hex[1..] : hex;
        (int r, int g, int b, float a) = ParseHexValue(value);

        return new(r, g, b, a);
    }

    /// <summary>
    ///     Creates color hexadecimal code.
    /// </summary>
    public override string ToString() => $"{this.red:X2}{this.green:X2}{this.blue:X2}";

    /// <summary>
    ///     Returns a color of RGBA.
    /// </summary>
    /// <param name="hex">Hex value.</param>
    /// <returns>An RGBA color.</returns>
    private static (int, int, int, float) ParseHexValue(string hex)
    {
        int r;
        int b;
        int g;
        int a = 255;

        switch (hex.Length)
        {
            // FFFF 
            case 4:
                a = 17 * HexValue(hex[3]);
                goto case 3;
            case 3:
                // F00
                r = 17 * HexValue(hex[0]);
                g = 17 * HexValue(hex[1]);
                b = 17 * HexValue(hex[2]);
                break;
            case 8:
                // FFFFFF00
                a = (16 * HexValue(hex[6])) + HexValue(hex[7]);
                goto case 6;
            case 6:
                r = (16 * HexValue(hex[0])) + HexValue(hex[1]);
                g = (16 * HexValue(hex[2])) + HexValue(hex[3]);
                b = (16 * HexValue(hex[4])) + HexValue(hex[5]);
                break;
            default:
                // String format is invalid.
                throw new FormatException("Hex value is invalid");
        }

        static int HexValue(char hex)
        {
            return Convert.ToInt32($"0x{hex}", 16);
        }

        return (r, g, b, a);
    }
}