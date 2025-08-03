using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Drawing;

public class MultiLine
{
    private readonly int _maxWidth;
    private readonly Func<string, Size> _lineTextSizeCalculator;
    private readonly string _originalText;
    private readonly IList<string> _lines = new List<string>();

    static Dictionary<string, string> MLCache = new Dictionary<string, string>();

    public MultiLine(int maxWidth, Func<string, Size> lineTextSizeCalculator, string text)
    {
        var key = maxWidth.ToString() + lineTextSizeCalculator.ToString() + text.GetHashCode() ;
        _originalText = text;
        _lineTextSizeCalculator = lineTextSizeCalculator;
        _maxWidth = maxWidth;

        if (MLCache.ContainsKey(key))
        {
            Text = MLCache[key];
            CalculateTextSize();
        }else{
            //System.Console.WriteLine("ML: "+ text.Substring(0, Math.Min(40, text.Length)));
            Calculate();
            MLCache[key] = Text;
            if (MLCache.Count > 50)
                MLCache.Clear();
        }
    }

    private void Calculate()
    {
        SplitByLine();
        CalculateMultiLineText();
        CalculateTextSize();
    }

    private void SplitByLine()
    {
        var offset = 0;
        while (offset < _originalText.Length)
        {
            var part = _originalText.Substring(offset);
            var length = Math.Max(1, BreakText(part, _maxWidth));
            var extraLength = 0;
            var spaceOffset = part.LastIndexOf(' ', length - 1, length / 2);
            if (spaceOffset > 0 && offset + length < _originalText.Length)
            {
                length = spaceOffset;
                extraLength = 1;
            }
            int croffset = part.IndexOf('\n');
            if (croffset >= 0 && croffset < length)
            {
                length = croffset;
                extraLength = 1;
            }
            AddLine(_originalText.Substring(offset, length + extraLength));
            offset += length + extraLength;
        }
    }

    private int BreakText(string text, int maxWidth)
    {
        if (maxWidth == 0)
            return 0;
        if (string.IsNullOrEmpty(text))
            return 0;
        var textWidth = _lineTextSizeCalculator(text).Width;
        if (textWidth < maxWidth)
            return text.Length;

        var averageCharacterWidth = text.Length/(float) textWidth;

        return (int) (maxWidth*averageCharacterWidth);
    }

    private void AddLine(string line)
    {
        _lines.Add(line.Replace("\n",""));
    }

    private void CalculateTextSize()
    {
        Size = new Size(_maxWidth, _lineTextSizeCalculator(Text).Height);
    }

    private void CalculateMultiLineText()
    {
        Text = _lines.Any() ? _lines.Aggregate((cur, res) => cur + "\r\n" + res) : string.Empty;
    }

    public int LineCount
    {
        get { return _lines.Count; }
    }

    public string Text { get; private set; }

    public Size Size { get; private set; }

    public override string ToString()
    {
        return Text;
    }
}
