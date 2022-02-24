using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceMQTTHandler
{
    public class LampStatus
    {
        public bool IsOn { get; set; } = false;
        public Color Color { get; set; } = Color.White;
        public ushort Brightness { get; set; } = 0;
        public int BrightnessInPercent { get
            {
                return Brightness * 100 / 255;
            }
            set
            {
                Brightness = (ushort)(value * 255 / 100);
            }
        }
        public int Effect { get; set; } = 0;

        public override string ToString()
        {
            string result = "";
            result += $"Is on: {IsOn}; ";
            result += $"Color: {Color.ToHexString()}; ";
            result += $"Brightness: {BrightnessInPercent}%; ";
            result += $"Current effect: {Effect}; ";
            return result;
        }
    }

    public static class ColorConverterExtensions
    {
        public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";
    }
}
