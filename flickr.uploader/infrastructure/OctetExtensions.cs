using System;

namespace flickr.uploader.infrastructure
{
    public static class OctetExtensions
    {
        public static string ToOctets(this double value)
        {
            return ToOctets((long) value);
        }
        public static string ToOctets(this long value)
        {
            var _1_KO = Math.Pow(2, 10);
            var _1_MO = Math.Pow(2, 20);
            var _1_GO = Math.Pow(2, 30);

            if (value >= _1_GO) {
                return $"{Math.Round(value / _1_GO, 1)} Go";
            }
            if (value >= _1_MO) {
                return $"{Math.Round(value / _1_MO, 1)} Mo";
            }
            if (value >= _1_KO) {
                return $"{Math.Round(value / _1_KO, 1)} Ko";
            }
            return $"{value} o";
        }
    }
}