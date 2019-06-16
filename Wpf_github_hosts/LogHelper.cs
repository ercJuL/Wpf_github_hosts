using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf_github_hosts
{
    public  class LogHelper
    {
        public  StringBuilder LogAllTxtBuilder = new StringBuilder(100);
        public Label LableComponent { get; set; }
        public TextBlock TextBlockComponent { get; set; }

        public enum InfoLevel
        {
            Debug = 0, // 调试信息
            Info = 1, // 给用户的提示
            Waring = 2, // 非致命错误
            Error = 3, //致命错误
        }

        public  void UpdateLog(string message, InfoLevel infoLevel)
        {
            var logStr = String.Format("{0:G} {1:G} {2}\n", DateTime.Now, infoLevel, message);
            if (infoLevel == InfoLevel.Info && LableComponent != null)
            {
                LableComponent.Content = logStr;
            }
            LogAllTxtBuilder.Append(logStr);
        }
    }
}
