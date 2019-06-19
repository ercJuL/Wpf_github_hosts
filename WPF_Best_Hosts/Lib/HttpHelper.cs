using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Wpf_github_hosts;

namespace WPF_Best_Hosts.Lib
{
    public static class HttpHelper
    {
        public static HtmlSearch GetHtmlTask(string postUrl, string pathSegment, object formData)
        {
            var responseHtml = postUrl.AppendPathSegment(pathSegment)
                .WithTimeout(10000)
                .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36")
                .PostUrlEncodedAsync(formData)
                .ReceiveString();
            return new HtmlSearch(responseHtml.Result);
        }

        public static string GetPingDataTask(string postUrl, string pathSegment, object formData)
        {
                var responseJson = postUrl.AppendPathSegment(pathSegment).SetQueryParam("t=ping").WithTimeout(10000)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36")
                    .PostUrlEncodedAsync(formData)
                    .ReceiveString();
                return responseJson.Result;
        }
    }
}