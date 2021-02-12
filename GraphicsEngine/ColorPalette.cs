using SharpDX;

namespace GraphicsEngine
{
    public static class ColorPalette
    {
        public static Color PolarNight1 => new Color( 46,  52,  64);
        public static Color PolarNight2 => new Color( 59,  66,  82);
        public static Color PolarNight3 => new Color( 67,  76,  94);
        public static Color PolarNight4 => new Color( 76,  86, 106);

        public static Color SnowStorm1  => new Color(216, 222, 233);
        public static Color SnowStorm2  => new Color(229, 233, 240);
        public static Color SnowStorm3  => new Color(236, 239, 244);

        public static Color Frost1      => new Color(143, 188, 187);
        public static Color Frost2      => new Color(136, 192, 208);
        public static Color Frost3      => new Color(129, 161, 193);
        public static Color Frost4      => new Color( 94, 129, 172);

        public static Color Aurora1     => new Color(191,  97, 106);
        public static Color Aurora2     => new Color(208, 135, 112);
        public static Color Aurora3     => new Color(235, 203, 139);
        public static Color Aurora4     => new Color(163, 190, 140);
        public static Color Aurora5     => new Color(180, 142, 173);
        
        public static Color[] ModelColors =>
            new Color[] { Frost2, Aurora4, Aurora3, Frost3, Aurora5, Frost1, Aurora2 };
    }
}