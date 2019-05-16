using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace AutoPublishNugetForNETCore
{
    /// <summary>
    /// 设置配置文件类
    /// </summary>
    public static class AppConfigSetting
    {

        /// <summary>
        /// nuget 站点地址
        /// </summary>
        public static string NugetUrl => AppSettingValue().EndsWith("/") ? AppSettingValue().TrimEnd('/') : AppSettingValue();


        /// <summary>
        /// pwd
        /// </summary>
        public static string Pwd => AppSettingValue();


        private static string AppSettingValue([CallerMemberName] string key = null)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}