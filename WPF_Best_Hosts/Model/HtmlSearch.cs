using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Wpf_github_hosts
{
    public class HtmlSearch
    {
        public HtmlSearch()
        {
        }

        public HtmlSearch(string html)
        {
            var html2 = Regex.Replace(html, "\\r", " ");
            html2 = Regex.Replace(html2, "\\n", " ");
            html2 = Regex.Replace(html2, "\\s{2,}", " ");
            GuidLocal = new Dictionary<string, string>();
            foreach (Match guidLocalMatch in Regex.Matches(html2, "<div id=\"(\\w+-\\w+-\\w+-\\w+-\\w+)\" class.*?<div class=\"col-2\" name=\"city\" serveruroup=\"\\d+\">(.*?)</div>"))
            {
                GuidLocal.Add(guidLocalMatch.Groups[1].Value,guidLocalMatch.Groups[2].Value);
            }
            var encodeMatch = Regex.Match(html, "<input type=\"hidden\" id=\"enkey\" value=\"([\\w\\|]+)\" />");
            Encode = encodeMatch.Groups[1].Value;
            if (Encode=="")
            {
                return;
            }
        }

        public Dictionary<string,string> GuidLocal { get; set; }
        public string Encode { get; set; }
    }
}