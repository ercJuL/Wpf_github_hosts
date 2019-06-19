using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Best_Hosts.Model
{
    public class UpdateDataParams
    {
        public UpdateDataParams(string guidLocalKey, string domain, string encode)
        {
            GuidLocalKey = guidLocalKey;
            Domain = domain;
            Encode = encode;
        }
        public string GuidLocalKey { get; set; }
        public string Domain { get; set; }
        public string Encode { get; set; }
    }
}
