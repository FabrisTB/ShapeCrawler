﻿using System;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace ShapeCrawler.Colors;

internal static class ColorTranslator
{
    private static readonly FieldInfo[] FieldInfoList;

    static ColorTranslator()
    {
        FieldInfoList = typeof(SKColors).GetFields(BindingFlags.Static | BindingFlags.Public);
    }

    internal static string HexFromName(string colorName)
    {
        if (colorName.Equals("white", StringComparison.CurrentCultureIgnoreCase))
        {
            return "FFFFFF";
        }

        var fieldInfo = FieldInfoList.First(fieldInfo => string.Equals(fieldInfo.Name, colorName, StringComparison.CurrentCultureIgnoreCase));
        var color = (SKColor)fieldInfo.GetValue(null) !;

        return color.ToString();
    }
}