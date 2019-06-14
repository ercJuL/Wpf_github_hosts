using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace Wpf_github_hosts
{
    public static class HttpHelper
    {
        public static async Task<string> GetHtmlTask(string postUrl, string pathSegment, object formData)
        {
            var responeseHtml = await postUrl.AppendPathSegment(pathSegment)
                .WithTimeout(10000)
                .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36")
                .PostUrlEncodedAsync(formData)
                .ReceiveString();
            return responeseHtml;
        }

        public static async Task<dynamic> GetPingDataTask(string postUrl, string pathSegment, object formData)
        {
            try
            {
                var responseJson = await postUrl.AppendPathSegment(pathSegment).SetQueryParam("t=ping").WithTimeout(10000)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36")
                    .PostUrlEncodedAsync(formData)
                    .ReceiveString();
                return responseJson;
            }
            catch (Exception e)
            {
                throw;
            }

            return new { };
        }
    }
}