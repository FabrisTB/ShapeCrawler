﻿using DocumentFormat.OpenXml.Packaging;

// ReSharper disable once CheckNamespace
namespace ShapeCrawler;

using A = DocumentFormat.OpenXml.Drawing;

/// <summary>
///     Represents a theme.
/// </summary>
public interface ITheme
{
    /// <summary>
    ///     Gets font scheme.
    /// </summary>
    IThemeFontScheme FontScheme { get; }

    /// <summary>
    ///     Gets the color scheme.
    /// </summary>
    IThemeColorScheme ColorScheme { get; }
}

internal sealed class Theme : ITheme
{
    private readonly OpenXmlPart sdkTypedOpenXmlPart;
    private readonly A.Theme aTheme;

    internal Theme(OpenXmlPart sdkTypedOpenXmlPart, A.Theme aTheme)
    {
        this.sdkTypedOpenXmlPart = sdkTypedOpenXmlPart;
        this.aTheme = aTheme;
    }

    public IThemeFontScheme FontScheme => new ThemeFontScheme(this.sdkTypedOpenXmlPart);

    public IThemeColorScheme ColorScheme => this.GetColorScheme();

    private IThemeColorScheme GetColorScheme()
    {
        return new ThemeColorScheme(this.aTheme.ThemeElements!.ColorScheme!);
    }
}