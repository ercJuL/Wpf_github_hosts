using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf_github_hosts
{
    public static class LogHelper
    {
        public static TextBlock LableComponent { get; set; }

        public static void UpdateLog(string message)
        {
            if (LableComponent != null)
            {
                LableComponent.Text = message;
            }
        }
    }
}
