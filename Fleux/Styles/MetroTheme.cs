namespace Fleux.Styles
{
    using System;
    using System.Drawing;
    using Core.GraphicsHelpers;

    /// <summary>
    /// Use as reference http://msdn.microsoft.com/en-us/library/ff769552(VS.92).aspx
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder",
        Justification = "Reviewed. Suppression is OK here.")]
    public static class MetroTheme
    {
        #region Brushes/Colors

        // Foreground color to single-out items of interest
        public static Color PhoneAccentBrush
        {
            get { return Color.FromArgb(40, 160, 220); }
        }

        public static Color PhoneForegroundBrush
        {
            get { return Color.White; }
        }

        public static Color PhoneBackgroundBrush
        {
            get { return Color.Black; }
        }

        public static Color PhoneInactiveBrush
        {
            get { return Color.Black; }
        }

        public static Color PhoneTextBoxBrush
        {
            get { return Color.White; }
        }

        public static Color PhoneSubtleBrush
        {
            get { return Color.FromArgb(153, 153, 153); }
        }

        public static Color PhoneContrastForegroundBrush
        {
            get { return Color.White; }
        }

        #endregion

        #region Font Families

        // TODO: public static string PhoneFontFamilyNormal { get { return "Segoe WP"; } }
        public static string PhoneFontFamilyNormal
        {
            get { return "Segoe WP"; }
        }

        public static string PhoneFontFamilyLight
        {
            get { return "Segoe WP Light"; }
        }

        public static string PhoneFontFamilySemiLight
        {
            get { return "Segoe WP SemiLight"; }
        }

        public static string PhoneFontFamilySemiBold
        {
            get { return "Segoe WP Semibold"; }
        }

        #endregion

        #region Font Size

        ////public static int PhoneFontSizeSmall { get { return 14; } }
        ////public static int PhoneFontSizeNormal { get { return 15; } }
        ////public static int PhoneFontSizeMedium { get { return 17; } }
        ////public static int PhoneFontSizeMediumLarge { get { return 19; } }
        ////public static int PhoneFontSizeLarge { get { return 24; } }
        ////public static int PhoneFontSizeExtraLarge { get { return 32; } }
        ////public static int PhoneFontSizeExtraExtraLarge { get { return 54; } }
        ////public static int PhoneFontSizeHuge { get { return 140; } }

        public static int PhoneFontSizeSmall
        {
            get { return 8; }
        }

        public static int PhoneFontSizeNormal
        {
            get { return 12; }
        }

        public static int PhoneFontSizeMedium
        {
            get { return 13; }
        }

        public static int PhoneFontSizeMediumLarge
        {
            get { return 16; }
        }

        public static int PhoneFontSizeLarge
        {
            get { return 19; }
        }

        public static int PhoneFontSizeExtraLarge
        {
            get { return 25; }
        }

        public static int PhoneFontSizeExtraExtraLarge
        {
            get { return 31; }
        }

        public static int PhoneFontSizeHuge
        {
            get { return 93; }
        }

        #endregion

        #region Thickness

        public static ThicknessStyle PhoneHorizontalMargin
        {
            get { return new ThicknessStyle(12, 0, 0); }
        }

        public static ThicknessStyle PhoneVerticalMargin
        {
            get { return new ThicknessStyle(0, 12, 0); }
        }

        public static ThicknessStyle PhoneMargin
        {
            get { return new ThicknessStyle(12, 0, 0); }
        }

        public static ThicknessStyle PhoneTouchTargetOverhang
        {
            get { return new ThicknessStyle(12, 0, 0); }
        }

        public static ThicknessStyle PhoneTouchTargetLargeOverhang
        {
            get { return new ThicknessStyle(12, 20, 0); }
        }

        public static ThicknessStyle PhoneBorderThickness
        {
            get { return new ThicknessStyle(3, 0, 0); }
        }

        public static ThicknessStyle PhoneStrokeThickness
        {
            get { return new ThicknessStyle(3, 0, 0); }
        }

        #endregion

        #region TextStyles

        public static TextStyle PhoneTextBlockBase
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilyNormal,
                    MetroTheme.PhoneFontSizeSmall,
                    MetroTheme.PhoneTextBoxBrush,
                    MetroTheme.PhoneHorizontalMargin);
            }
        }

        public static TextStyle PhoneTextNormalStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilyNormal,
                    MetroTheme.PhoneFontSizeNormal,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextTitle1Style
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeExtraExtraLarge,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextTitle2Style
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeLarge,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextTitle3Style
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeMedium,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextLargeStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeLarge,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextExtraLargeStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeExtraLarge,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextGroupHeaderStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiLight,
                    MetroTheme.PhoneFontSizeLarge,
                    MetroTheme.PhoneSubtleBrush);
            }
        }

        public static TextStyle PhoneTextSmallStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilyNormal,
                    MetroTheme.PhoneFontSizeSmall,
                    MetroTheme.PhoneSubtleBrush);
            }
        }

        public static TextStyle PhoneTextContrastStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiBold,
                    MetroTheme.PhoneFontSizeNormal,
                    MetroTheme.PhoneContrastForegroundBrush);
            }
        }

        public static TextStyle PhoneTextAccentStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiBold,
                    MetroTheme.PhoneFontSizeNormal,
                    MetroTheme.PhoneAccentBrush);
            }
        }

        #endregion

        #region additional text styles

        public static TextStyle PhoneTextPageTitle1Style
        {
            get
            {
                var r = new TextStyle(MetroTheme.PhoneFontFamilySemiBold, 8, MetroTheme.PhoneForegroundBrush);
                r.Thickness = new ThicknessStyle(0, 20, 0);
                return r;
            }
        }

        public static TextStyle PhoneTextPageTitle2Style
        {
            get
            {
                var r = new TextStyle(MetroTheme.PhoneFontFamilySemiLight, 25, MetroTheme.PhoneForegroundBrush);
                r.Thickness = new ThicknessStyle(0, 20, 0);
                return r;
            }
        }

        public static TextStyle TileTextStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilySemiBold,
                    7,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        #endregion

        #region Panorama

        public static TextStyle PhoneTextPanoramaTitleStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilyLight,
                    65,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        public static TextStyle PhoneTextPanoramaSectionTitleStyle
        {
            get
            {
                return new TextStyle(
                    MetroTheme.PhoneFontFamilyLight,
                    MetroTheme.PhoneFontSizeExtraLarge,
                    MetroTheme.PhoneForegroundBrush);
            }
        }

        // TitleHeader
        public static Action<IDrawingGraphics, string, string> DrawPanoramaTitleAction
        {
            get
            {
                return (g, title, subtitle) => g
                                                   .Style(MetroTheme.PhoneTextPanoramaTitleStyle).Bold(false)
                                                   .MoveX(0).MoveY(-90).DrawText(title)
                                                   .MoveX(0).MoveY(g.Bottom).Style(
                                                       MetroTheme.PhoneTextPanoramaSectionTitleStyle).DrawText(subtitle)
                                                   .MoveY(g.Bottom + 20);
            }
        }

        public static Color PanoramaNormalBrush
        {
            get { return Color.FromArgb(255, 255, 255); }   
        }

        #endregion
    }
}