using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XIVAuras.Helpers
{
    public class TextTagFormatter
    {
        public static Regex TextTagRegex { get; } = new Regex(@"\[(\w*)(:\w)?\.?(\d+)?\]", RegexOptions.Compiled);

        private string _format;
        private Dictionary<string, FieldInfo> _fields;
        private object _source;
        private int _rounding;

        public TextTagFormatter(
            object source,
            string format,
            int rounding,
            Dictionary<string, FieldInfo> fields)
        {
            _source = source;
            _format = format;
            _rounding = rounding;
            _fields = fields;
        }

        public string Evaluate(Match m)
        {
            if (m.Groups.Count != 4)
            {
                return m.Value;
            }

            string key = m.Groups[1].Value;
            string? value = null;
            
            if (_fields.ContainsKey(key))
            {
                object? propValue = _fields[key].GetValue(_source);

                if (propValue is null)
                {
                    return string.Empty;
                }

                if (propValue is float f)
                {
                    int decimals = int.TryParse(m.Groups[3].Value, out int dec) ? dec : 0;
                    value = m.Groups[2].Value switch
                    {
                        ":k" => KiloFormat(f, _format, decimals, _rounding) ?? m.Value,
                        ":t" => TimeFormat(f, _rounding),
                        _    => FloatFormat(f, _format, decimals, _rounding)
                    };
                }
                else
                {
                    value = propValue?.ToString();
                    if (!string.IsNullOrEmpty(value) &&
                        int.TryParse(m.Groups[3].Value, out int trim) &&
                        trim < value.Length)
                    {
                        value = propValue?.ToString().AsSpan(0, trim).ToString();
                    }
                }
            }

            return value ?? m.Value;
        }

        private static string FloatFormat(float value, string format, int decimals, int rounding)
        {
            int m = (int)Math.Pow(10, decimals);

            double temp = rounding switch
            {
                0 => Math.Truncate(value * m),
                1 => Math.Ceiling(value * m),
                2 => Math.Round(value * m),
                _ => Math.Truncate(value * m)
            } / m;

            return temp.ToString($"{format}{decimals}", CultureInfo.InvariantCulture);
        }

        private static string TimeFormat(float seconds, int rounding) => seconds switch
        {
            > 3600 => $"{(int)seconds / 3600:0}:{((int)seconds % 3600) / 60:00}:{(int)seconds % 60:00}",
            > 60   => $"{(int)seconds / 60:0}:{(int)seconds % 60:00}",
            _      => FloatFormat(seconds, "F", 0, rounding)
        };

        private static string KiloFormat(float num, string format, int decimals, int rounding) => num switch
        {
            >= 1000000 => FloatFormat(num / 1000000f, format, decimals, rounding) + "M",
            >= 1000 => FloatFormat(num / 1000f, format, decimals, rounding) + "K",
            _ => FloatFormat(num, format, decimals, rounding)
        };
    }
}