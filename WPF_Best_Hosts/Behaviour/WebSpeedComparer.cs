using System;
using System.Collections;
using WPF_Best_Hosts.Lib;

namespace WPF_Best_Hosts.Behaviour
{
    public class WebSpeedComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var xNum = Utils.InverseHumanReadableByteCount((string) x);
            var yNum = Utils.InverseHumanReadableByteCount((string) y);

            var result = xNum - yNum;
            return result == 0 ? 0 : (int) (Math.Abs(result) / result);
        }
    }
}