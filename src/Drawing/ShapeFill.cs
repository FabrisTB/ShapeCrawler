using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ShapeCrawler.Extensions;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Drawing;

internal sealed class ShapeFill(OpenXmlCompositeElement openXmlCompositeElement): IShapeFill
{
    private SlidePictureImage? pictureImage;
    private A.SolidFill? aSolidFill;
    private A.GradientFill? aGradFill;
    private A.PatternFill? aPatternFill;
    private A.BlipFill? aBlipFill;

    public string? Color
    {
        get
        {
            this.aSolidFill = openXmlCompositeElement.GetFirstChild<A.SolidFill>();
            if (this.aSolidFill != null)
            {
                var aRgbColorModelHex = this.aSolidFill.RgbColorModelHex;
                if (aRgbColorModelHex != null)
                {
                    return aRgbColorModelHex.Val!.ToString();
                }

                return this.ColorHexOrNullOf(this.aSolidFill.SchemeColor!.Val!);
            }

            return null;
        }
    }

    public double Alpha
    {
        get
        {
            const int defaultAlphaPercentages = 100;
            this.aSolidFill = openXmlCompositeElement.GetFirstChild<A.SolidFill>();
            if (this.aSolidFill != null)
            {
                var aRgbColorModelHex = this.aSolidFill.RgbColorModelHex;
                if (aRgbColorModelHex != null)
                {
                    var alpha = aRgbColorModelHex.Elements<A.Alpha>().FirstOrDefault();
                    return alpha?.Val?.Value / 1000d ?? defaultAlphaPercentages;
                }

                var schemeColor = this.aSolidFill.SchemeColor!;
                var schemeAlpha = schemeColor.Elements<A.Alpha>().FirstOrDefault();
                return schemeAlpha?.Val?.Value / 1000d ?? defaultAlphaPercentages;
            }

            return defaultAlphaPercentages;
        }
    }

    public double LuminanceModulation
    {
        get
        {
            const double luminanceModulation = 100;
            this.aSolidFill = openXmlCompositeElement.GetFirstChild<A.SolidFill>();
            if (this.aSolidFill != null)
            {
                var aRgbColorModelHex = this.aSolidFill.RgbColorModelHex;
                if (aRgbColorModelHex != null)
                {
                    return luminanceModulation;
                }

                var schemeColor = this.aSolidFill.SchemeColor!;
                var schemeAlpha = schemeColor.Elements<A.LuminanceModulation>().FirstOrDefault();
                return schemeAlpha?.Val?.Value / 1000d ?? luminanceModulation;
            }

            return luminanceModulation;
        }
    }

    public double LuminanceOffset
    {
        get
        {
            const double defaultValue = 0;
            this.aSolidFill = openXmlCompositeElement.GetFirstChild<A.SolidFill>();
            if (this.aSolidFill != null)
            {
                var aRgbColorModelHex = this.aSolidFill.RgbColorModelHex;
                if (aRgbColorModelHex != null)
                {
                    return defaultValue;
                }

                var schemeColor = this.aSolidFill.SchemeColor!;
                var schemeAlpha = schemeColor.Elements<A.LuminanceOffset>().FirstOrDefault();
                return schemeAlpha?.Val?.Value / 1000d ?? defaultValue;
            }

            return defaultValue;
        }
    }

    public IImage? Picture => this.GetPictureImage();

    public FillType Type => this.GetFillType();

    public void SetPicture(Stream image)
    {
        var openXmlPart = openXmlCompositeElement.Ancestors<OpenXmlPartRootElement>().First().OpenXmlPart!;
        if (this.Type == FillType.Picture)
        {
            this.pictureImage!.Update(image);
        }
        else
        {
            (var rId, _) = openXmlPart.AddImagePart(image, "image/png");

            // This could be refactored to DRY vs SlideShapes.CreatePPicture.
            // In the process, the image could be de-duped also.
            this.aBlipFill = new A.BlipFill();
            var aStretch = new A.Stretch();
            aStretch.Append(new A.FillRectangle());
            this.aBlipFill.Append(new A.Blip { Embed = rId });
            this.aBlipFill.Append(aStretch);

            openXmlCompositeElement.Append(this.aBlipFill);

            this.aSolidFill?.Remove();
            this.aBlipFill = null;
            this.aGradFill?.Remove();
            this.aGradFill = null;
            this.aPatternFill?.Remove();
            this.aPatternFill = null;
        }
    }

    public void SetColor(string hex)
    {
        this.InitSolidFillOr();
        openXmlCompositeElement.AddSolidFill(hex);
    }

    public void SetNoFill()
    {
        this.InitSolidFillOr();
        openXmlCompositeElement.AddNoFill();
    }

    private void InitSolidFillOr()
    {
        this.aSolidFill = openXmlCompositeElement.GetFirstChild<A.SolidFill>();
        if (this.aSolidFill == null)
        {
            this.aGradFill = openXmlCompositeElement!.GetFirstChild<A.GradientFill>();
            if (this.aGradFill == null)
            {
                this.InitPictureFillOr();
            }
        }
    }
    
    private bool HasSolidFill()
    {
        return openXmlCompositeElement.GetFirstChild<A.SolidFill>() != null;
    }

    private bool HasGradientFill()
    {
        return openXmlCompositeElement.GetFirstChild<A.GradientFill>() != null;
    }

    private bool HasBlipFill()
    {
        return openXmlCompositeElement.GetFirstChild<A.BlipFill>() != null;
    }

    private bool HasPatternFill()
    {
        return openXmlCompositeElement.GetFirstChild<A.PatternFill>() != null;
    }

    private FillType GetFillType()
    {
        if (this.HasSolidFill())
        {
            return FillType.Solid;
        }

        if (this.HasGradientFill())
        {
            return FillType.Gradient;
        }

        if (this.HasBlipFill())
        {
            return FillType.Picture;
        }

        if (this.HasPatternFill())
        {
            return FillType.Pattern;
        }

        if (openXmlCompositeElement.Ancestors<P.Shape>().FirstOrDefault()?.UseBackgroundFill is not null)
        {
            return FillType.SlideBackground;
        }

        return FillType.NoFill;
    }
    
    private string? ColorHexOrNullOf(string schemeColor)
    {
        var openXmlPart = openXmlCompositeElement.Ancestors<OpenXmlPartRootElement>().First().OpenXmlPart!;
        var aColorScheme = openXmlPart switch
        {
            SlidePart sdkSlidePart => sdkSlidePart.SlideLayoutPart!.SlideMasterPart!.ThemePart!.Theme.ThemeElements!
                .ColorScheme!,
            SlideLayoutPart sdkSlideLayoutPart => sdkSlideLayoutPart.SlideMasterPart!.ThemePart!.Theme.ThemeElements!
                .ColorScheme!,
            _ => ((SlideMasterPart)openXmlPart).ThemePart!.Theme.ThemeElements!.ColorScheme!
        };

        var aColor2Type = aColorScheme.Elements<A.Color2Type>().FirstOrDefault(c => c.LocalName == schemeColor);
        var hex = aColor2Type?.RgbColorModelHex?.Val?.Value ?? aColor2Type?.SystemColor?.LastColor?.Value;

        if (hex != null)
        {
            return hex;
        }

        return null;
    }

    private void InitPictureFillOr()
    {
        this.aBlipFill = openXmlCompositeElement.GetFirstChild<A.BlipFill>();

        if (this.aBlipFill is not null)
        {
            var image = new SlidePictureImage(this.aBlipFill.Blip!);
            this.pictureImage = image;
        }
    }

    private SlidePictureImage? GetPictureImage()
    {
        this.InitSolidFillOr();

        return this.pictureImage;
    }
}