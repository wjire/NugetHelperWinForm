using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace NugetHelperWinForm
{
    /// <summary>
    /// 设置配置文件类
    /// </summary>
    public static class AppConfigSetting
    {
        /// <summary>
        /// 小工具所在文件夹的物理路径(含文件夹名)
        /// </summary>
        public static string ToolPath => AppSettingValue();

        /// <summary>
        /// nuget 站点地址
        /// </summary>
        public static string NugetUrl => AppSettingValue();

        /// <summary>
        /// 获取版本号的接口地址
        /// </summary>
        public static string ApiUri => AppSettingValue();

        /// <summary>
        /// pwd
        /// </summary>
        public static string Pwd => AppSettingValue();


        /// <summary>
        /// Nuget Packages 物理路径
        /// </summary>
        public static string PackagesUrl => AppSettingValue();



        private static string AppSettingValue([CallerMemberName] string key = null)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}