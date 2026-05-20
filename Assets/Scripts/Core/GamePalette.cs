using UnityEngine;

namespace NeonDrift.Core
{
    public static class GamePalette
    {
        public static readonly Color DeepSpace = Hex("10131d");
        public static readonly Color Ink = Hex("161a27");
        public static readonly Color Panel = Hex("202638");
        public static readonly Color PanelSoft = Hex("2a3248");
        public static readonly Color Text = Hex("f5f7ff");
        public static readonly Color MutedText = Hex("aeb8d6");
        public static readonly Color Cyan = Hex("55d7ff");
        public static readonly Color Mint = Hex("5ff3b4");
        public static readonly Color Coral = Hex("ff637d");
        public static readonly Color Gold = Hex("ffd166");
        public static readonly Color Violet = Hex("a78bfa");

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
