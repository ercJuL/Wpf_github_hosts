using System;
using System.Text.RegularExpressions;

namespace WPF_Best_Hosts.Lib
{
    public static class Utils
    {
        public static string HumanReadableByteCount(long bytes, bool isDa)
        {
            var unit = isDa ? 1000 : 1024;
            if (bytes < unit) return bytes + " B";
            var exp = (int) (Math.Log(bytes) / Math.Log(unit));
            var pre = (isDa ? "kMGTPE" : "KMGTPE")[exp - 1] + (isDa ? "" : "i");
            return string.Format("{0:F1} {1}B", bytes / Math.Pow(unit, exp), pre);
        }

        public static long InverseHumanReadableByteCount(string hrString)
        {
            var pre = Regex.Match(hrString, " [KMGTPE]?i?B", RegexOptions.IgnoreCase);
            if (!pre.Success) return -1;
            var num = Convert.ToDouble(hrString.TrimStart().Split(' ')[0]);
            var preStr = pre.Value.Trim().ToUpper();
            if (preStr == "B") return (long) num;
            var unit = preStr.Contains("i") ? 1024 : 1000;
            var exp = "KMGTPE".IndexOf(preStr[0]) + 1;
            return (long) (num * Math.Pow(unit, exp));
        }

        public static decimal StringToNum(object x)
        {
            var strX = Regex.Replace((string) x, @"[^\d.\d]", "");
            if (Regex.IsMatch(strX, @"^[+-]?\d+[.]?\d*$")) return decimal.Parse(strX);
            return decimal.MaxValue;
        }
    }
}