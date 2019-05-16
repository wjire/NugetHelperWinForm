using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPublishNugetForNETCore
{
    internal class Program
    {
        //项目文件夹的绝对路径,含文件夹名
        private static string projectFileName;

        //项目名称
        private static string targetName;

        //nuget 站点
        private static readonly string nugetUrl = AppConfigSetting.NugetUrl;

        //pwd
        private static readonly string pwd = AppConfigSetting.Pwd;

        //版本号
        private static string version = string.Empty;

        private static void Main(string[] args)
        {
            projectFileName = args[0];
            targetName = args[1];
            version = PackageVersionHelper.GetNewVersion(targetName);
            ProcessCmd();
        }


        /// <summary>
        /// 执行 windows cmd 命令
        /// </summary>
        private static void ProcessCmd()
        {
            Process proc = new Process();
            string strOuput = null;
            try
            {
                proc.StartInfo.FileName = "cmd.exe";

                //是否使用操作系统shell启动
                proc.StartInfo.UseShellExecute = false;

                //接受来自调用程序的输入信息
                proc.StartInfo.RedirectStandardInput = true;

                //输出信息
                proc.StartInfo.RedirectStandardOutput = true;

                //输出错误
                proc.StartInfo.RedirectStandardError = true;

                //不显示程序窗口
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();

                //构造命令
                StringBuilder sb = new StringBuilder();

                sb.Append(CreateCmd());

                //退出
                sb.Append("&exit");

                //向cmd窗口发送输入信息
                proc.StandardInput.WriteLine(sb.ToString());
                proc.StandardInput.AutoFlush = true;

                strOuput = proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                proc.WaitForExit();
                proc.Close();
                Console.WriteLine(strOuput);
            }
        }

        /// <summary>
        /// 构造cmd命令
        /// </summary>
        /// <returns></returns>
        private static string CreateCmd()
        {
            StringBuilder sb = new StringBuilder();
            projectFileName = projectFileName.Trim('\\');

            //打包
            sb.Append($"dotnet pack {projectFileName} -p:packageversion={version}");

            //发布
            sb.Append($@"&&dotnet nuget push {projectFileName}\bin\debug\*.nupkg -k {pwd} -s {nugetUrl}/nuget");

            //删除生成的文件
            sb.Append($"&&del {projectFileName}\\bin\\debug\\*.nupkg");

            return sb.ToString();
        }
    }
}
