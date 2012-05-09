namespace Fleux.Core.GraphicsHelpers
{
    using System.Drawing;
    using Core.NativeHelpers;
    using Styles;
    using UIElements;

    /// <summary>
    /// This interface should be implemented by Render elements 
    /// or dimension calculators.
    /// It provides a Fluent API
    /// </summary>
    public interface IDrawingGraphics
    {
        // Getters
        // Bounds (may change while it's drawing)
        int Right { get; } // Based on actual drawing extends. Not Scaled
        
        int Bottom { get; } // Based on actual drawing extends. Not Scaled
        
        int Width { get; } // Defined Width. Not Scaled 

        int X { get; } // Not Scaled
        
        int Y { get; } // Not Scaled

        Graphics Graphics { get; }

        Rectangle VisibleRect { get; } // Logical visible rect

        /**
         * Retrieves current offscreen buffer content into the bitmap.
         * If platform allows, it'll just use a transparent fill into bitmap (to improve performance)
         */
        ClipBuffer GetClipBuffer(Rectangle rectangle, Bitmap bitmap);

        /**
         * Retrieves current offscreen buffer content into the bitmap.
         * This'll always return opaque bitmap filled with current offscreen content
         */
        ClipBuffer GetOpaqueClipBuffer(Rectangle rectangle, Bitmap bitmap);

        IDrawingGraphics Color(Color color); // Color for Text, Lines, Rectangles, etc.
        
        IDrawingGraphics PenWidth(int dx);
        
        IDrawingGraphics FontName(string fontName);
        
        IDrawingGraphics FontSize(int size);
        
        IDrawingGraphics Bold(bool enable);
        
        IDrawingGraphics Italic(bool enable);
        
        IDrawingGraphics MoveTo(int x, int y);
        
        IDrawingGraphics MoveX(int x);
        
        IDrawingGraphics MoveY(int y);
        
        IDrawingGraphics MoveRelX(int dx);
        
        IDrawingGraphics MoveRelY(int dy);
        
        IDrawingGraphics MoveRel(int dx, int dy);
        
        IDrawingGraphics Style(TextStyle style);
        
        // Standard Graphics Drawing actions
        
        // Lines
        IDrawingGraphics DrawLine(int x1, int y1, int x2, int y2);
        
        IDrawingGraphics DrawLineTo(int x2, int y2);
        
        // Rectangles
        IDrawingGraphics DrawRectangle(int x1, int y1, int x2, int y2);
        
        IDrawingGraphics DrawRectangle(Rectangle r);
        
        IDrawingGraphics FillRectangle(int x1, int y1, int x2, int y2);
        
        IDrawingGraphics FillRectangle(Rectangle r);
        
        // Ellipses
        IDrawingGraphics DrawEllipse(int x1, int y1, int x2, int y2);
        
        IDrawingGraphics DrawEllipse(Rectangle r);
        
        IDrawingGraphics FillEllipse(int x1, int y1, int x2, int y2);
        
        IDrawingGraphics FillEllipse(Rectangle r);

        // Images
        IDrawingGraphics DrawImage(Image image, int x, int y);
        
        IDrawingGraphics DrawImage(Image image, int x1, int y1, int width, int height);
        
        IDrawingGraphics DrawImage(Image image, Rectangle r);
        
        IDrawingGraphics DrawImage(Image image, Rectangle destRect, Rectangle sourceRect);
        
        IDrawingGraphics DrawImage(string resourceName, int x, int y);
        
        IDrawingGraphics DrawImage(string resourceName, int x1, int y1, int width, int height);
        
        IDrawingGraphics DrawImage(string resourceName, Rectangle r);
        
        IDrawingGraphics DrawImage(string resourceName, Rectangle destRect, Rectangle sourceRect);

        IDrawingGraphics DrawImageFile(string fileName, int x, int y);
        
        IDrawingGraphics DrawImageFile(string fileName, int x1, int y1, int width, int height);
        
        IDrawingGraphics DrawImageFile(string fileName, Rectangle r);
        
        IDrawingGraphics DrawImageFile(string fileName, Rectangle destRect, Rectangle sourceRect);

        // AlphaImages
        IDrawingGraphics DrawAlphaImage(IImageWrapper image, int x, int y);
        
        IDrawingGraphics DrawAlphaImage(IImageWrapper image, int x1, int y1, int width, int height);

        IDrawingGraphics DrawAlphaImage(IImageWrapper image, Rectangle r);

        IDrawingGraphics DrawAlphaImage(IImageWrapper image, Rectangle destRect, Rectangle sourceRect);
        
        IDrawingGraphics DrawAlphaImage(string resourceName, int x, int y);
        
        IDrawingGraphics DrawAlphaImage(string resourceName, int x1, int y1, int width, int height);
        
        IDrawingGraphics DrawAlphaImage(string resourceName, Rectangle r);
        
        IDrawingGraphics DrawAlphaImage(string resourceName, Rectangle destRect, Rectangle sourceRect);
        
        IDrawingGraphics DrawAlphaImageFile(string fileName, int x, int y);
        
        IDrawingGraphics DrawAlphaImageFile(string fileName, int x1, int y1, int width, int height);
        
        IDrawingGraphics DrawAlphaImageFile(string fileName, Rectangle r);
        
        IDrawingGraphics DrawAlphaImageFile(string fileName, Rectangle destRect, Rectangle sourceRect);

        // Text
        IDrawingGraphics DrawText(string text);
        
        IDrawingGraphics DrawText(string text, int width);
        
        IDrawingGraphics DrawTextEllipsis(string text, int width);
        
        IDrawingGraphics DrawMultiLineText(string text, int width);
        
        IDrawingGraphics DrawMultiLineText(string text, int width, int height);
        
        // Between currentX and currentX + width
        IDrawingGraphics DrawCenterText(string text, int width);
        /**
         * Center text in a box
         */
        IDrawingGraphics DrawCenterText(string text, int width, int height);
        
        IDrawingGraphics DrawRightText(string text); // Aligned to currentX

        int CalculateMultilineTextHeight(string text, int width);

        Size CalculateTextSize(string text);

        int CalculateWidth(int x);

        int CalculateHeight(int y);

        int CalculateX(int x);

        int CalculateY(int y);

        Rectangle CalculateRect(Rectangle logicalRect);

        IDrawingGraphics CreateChild(Point innerlocation, double scaling, Point transformationCenter);

        IDrawingGraphics CreateChild(Point innerlocation);
    }
}
