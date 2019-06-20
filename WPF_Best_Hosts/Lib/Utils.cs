using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Best_Hosts.Lib
{
    public static class Utils
    {
        public static String HumanReadableByteCount(long bytes, bool isDa)

        {

            int unit = isDa ? 1000 : 1024;

            if (bytes < unit) return bytes + " B";

            int exp = (int)(Math.Log(bytes) / Math.Log(unit));

            String pre = (isDa ? "kMGTPE" : "KMGTPE")[exp - 1] + (isDa ? "" : "i");

            return String.Format("{0:F1} {1}B", bytes / Math.Pow(unit, exp), pre);

        }
    }
}
