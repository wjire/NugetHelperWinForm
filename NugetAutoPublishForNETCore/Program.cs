using System;
using NugetHelperWinFormForNETCore;

namespace NugetAutoPublishForNETCore
{
    internal class Program
    {

        //项目文件夹的绝对路径,含文件夹名
        private static string projectFileName;

        //项目名称
        private static string targetName;

        //nuget 站点,如: http://www.mynuget.com
        private static readonly string nugetUrl = AppConfigSetting.NugetUrl;

        //pwd
        private static readonly string pwd = AppConfigSetting.Pwd;

        private static string version = string.Empty;

        private static void Main(string[] args)
        {
            //接收通过 window cmd 命令行运行该程序时传入的参数(这些参数是通过VS编译器的外部工具传入的)
            projectFileName = args[0];
            targetName = args[1];
        }
    }
}
