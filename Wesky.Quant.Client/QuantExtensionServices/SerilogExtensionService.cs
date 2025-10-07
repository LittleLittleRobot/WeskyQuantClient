using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wesky.Quant.Client.QuantExtensionServices
{
    public static  class SerilogExtensionService
    {
        public static void AddSerilogConfiguration(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // 初始化本地日志文件写入格式
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Async(a => a.File(System.Environment.CurrentDirectory + $"\\Logs\\.log", rollingInterval: RollingInterval.Hour,
                                                outputTemplate: "{NewLine}DateTime:{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}LogLevel:{Level}{NewLine}Message:{Message}{NewLine}{Exception}",
                                                retainedFileCountLimit: 200))
                .CreateLogger();
        }

    }
}
