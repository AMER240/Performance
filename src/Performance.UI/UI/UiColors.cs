using System.Drawing;

namespace Performance.UI
{
    /// <summary>
    /// Application color palette - Professional light green theme
    /// </summary>
    public static class UiColors
    {
        // Main color palette
        public static readonly Color LightGreen = Color.FromArgb(235, 244, 221);      // #EBF4DD - Lightest
        public static readonly Color MediumGreen = Color.FromArgb(144, 171, 139);     // #90AB8B - Medium
        public static readonly Color DarkGreen = Color.FromArgb(90, 120, 99);         // #5A7863 - Dark
        public static readonly Color DarkestGreen = Color.FromArgb(59, 73, 83);       // #3B4953 - Darkest

        // Semantic colors
        public static readonly Color Background = LightGreen;
        public static readonly Color CardBackground = Color.White;
        public static readonly Color PanelBackground = MediumGreen;
        public static readonly Color AccentColor = DarkGreen;
        public static readonly Color DarkAccent = DarkestGreen;

        // Text colors
        public static readonly Color PrimaryText = DarkestGreen;
        public static readonly Color SecondaryText = DarkGreen;
        public static readonly Color LightText = Color.White;
        public static readonly Color DisabledText = Color.FromArgb(150, 150, 150);

        // Button colors
        public static readonly Color ButtonPrimary = DarkGreen;
        public static readonly Color ButtonSecondary = MediumGreen;
        public static readonly Color ButtonHover = DarkestGreen;
        public static readonly Color ButtonText = Color.White;

        // Status colors (with green tints)
        public static readonly Color Success = Color.FromArgb(76, 175, 80);           // Green
        public static readonly Color Warning = Color.FromArgb(255, 152, 0);           // Orange
        public static readonly Color Error = Color.FromArgb(244, 67, 54);             // Red
        public static readonly Color Info = Color.FromArgb(33, 150, 243);             // Blue

        // Priority colors (adjusted for theme)
        public static readonly Color PriorityHigh = Error;
        public static readonly Color PriorityMedium = Warning;
        public static readonly Color PriorityLow = Success;

        // Status colors for tasks
        public static readonly Color StatusTodo = Info;
        public static readonly Color StatusInProgress = Warning;
        public static readonly Color StatusDone = Success;

        // Grid colors
        public static readonly Color GridHeader = DarkGreen;
        public static readonly Color GridRow = Color.White;
        public static readonly Color GridAlternateRow = Color.FromArgb(245, 250, 242);
        public static readonly Color GridBorder = MediumGreen;
        public static readonly Color GridSelection = DarkGreen;

        // Input colors
        public static readonly Color InputBackground = Color.White;
        public static readonly Color InputBorder = MediumGreen;
        public static readonly Color InputFocus = DarkGreen;
        public static readonly Color InputText = PrimaryText;

        // Helper method for lighter/darker shades
        public static Color Lighten(Color color, float percent)
        {
            return Color.FromArgb(
                color.A,
                (int)System.Math.Min(255, color.R + (255 - color.R) * percent),
                (int)System.Math.Min(255, color.G + (255 - color.G) * percent),
                (int)System.Math.Min(255, color.B + (255 - color.B) * percent)
            );
        }

        public static Color Darken(Color color, float percent)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1 - percent)),
                (int)(color.G * (1 - percent)),
                (int)(color.B * (1 - percent))
            );
        }
    }
}
