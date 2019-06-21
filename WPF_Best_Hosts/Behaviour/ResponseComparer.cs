using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WPF_Best_Hosts.Lib;

namespace WPF_Best_Hosts.Behaviour
{
    public class ResponseComparer: IComparer
    {
        public int Compare(object x, object y)
        {
            var xNum = Utils.StringToNum(x);
            var yNum = Utils.StringToNum(y);

            var result = xNum - yNum;
            return result == 0 ? 0 : (int) (Math.Abs(result) / result);
        }

        
    }
}
