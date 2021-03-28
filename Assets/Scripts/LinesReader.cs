using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Structs;
using UnityEngine;

public static class LinesReader
{
    public static List<Line> GetLines()
    {
        var textLines = ReadFile();
        var lines = ParseTextLines(textLines);
        return lines;
    }
    
    private static string[] ReadFile()
    {
        var asset = Resources.Load<TextAsset>( "LinesConfig");
        var lines = asset.text.Split('\n');
        return lines;
    }

    private static List<Line> ParseTextLines(string[] textLines)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        return textLines.Skip(1)
            .Select(textLine => textLine.Split(','))
            .Select(coordinates => new Line {
                Start = new Vector2(float.Parse(coordinates[0]), float.Parse(coordinates[1])),
                End = new Vector2(float.Parse(coordinates[2]), float.Parse(coordinates[3]))
            })
            .ToList();
    }
}
