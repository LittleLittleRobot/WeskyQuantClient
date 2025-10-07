using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wesky.Quant.Client.Infrastructure.Tools
{
    public class ReadJsonHelper
    {
        private static IConfiguration _config;

        public ReadJsonHelper()
        {
            string contentPath = $"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}";

            _config = new ConfigurationBuilder()
                .SetBasePath(contentPath)
               .Add(new JsonConfigurationSource { Path = "weskyquant.json", Optional = false, ReloadOnChange = true })//直接读目录里的json文件
               .Build();
        }

        /// <summary>
        /// 读取指定节点的字符串
        /// </summary>
        /// <param name="sessions"></param>
        /// <returns></returns>
        public static string Read(params string[] sessions)
        {
            try
            {
                if (sessions.Any())
                {
                    return _config[string.Join(":", sessions)];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }


        public static object ReadObject(string sessions)
        {
            try
            {
                if (sessions.Any())
                {
                    return _config[sessions];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        /// <summary>
        /// 读取实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public static List<T> Read<T>(params string[] session)
        {
            List<T> list = new List<T>();
            _config.Bind(string.Join(":", session), list);
            return list;
        }

        public static string GetGenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}
