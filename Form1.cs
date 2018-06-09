﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NugetHelperWinForm
{
    public partial class Form1 : Form
    {
        //AssemblyInfo.cs文件的绝对路径
        private readonly string assemblyinfoPath;

        //项目文件夹的绝对路径
        private readonly string projectDir;

        //项目文件夹的绝对路径,含文件夹名
        private readonly string projectFileName;

        //项目名称
        private readonly string targetName;

        //小工具所在的文件夹的绝对路径,不含文件夹名
        private readonly string toolPath = AppConfigSetting.ToolPath;

        //nuget 站点,如: http://www.mynuget.com
        private readonly string nugetUrl = AppConfigSetting.NugetUrl;

        //api 接口
        private readonly string apiUri = AppConfigSetting.ApiUri;

        //pwd
        private readonly string pwd = AppConfigSetting.Pwd;

        //Nuget Packages 物理路径
        private readonly string packagesUrl = AppConfigSetting.PackagesUrl;


        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string[] args)
        {
            InitializeComponent();

            projectFileName = args[0];
            targetName = args[1];
            projectDir = args[2];
            assemblyinfoPath = args[2] + @"Properties\AssemblyInfo.cs";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string url = nugetUrl + apiUri + targetName;
            lblName.Text = targetName;
            try
            {
                //通过nuget站点的接口获取最高版本号
                //string version = HttpGet(url).Trim('"');

                //通过访问nuget站点的共享文件夹获取最高版本号
                var version = GetMaxVersion();

                if (string.IsNullOrWhiteSpace(version))
                {
                    lblVersion.Text = "第一次上传该项目";
                    txtVersion.Text = "1.0.0";
                }
                else
                {
                    lblVersion.Text = version;
                    txtVersion.Text = UpdateVersion(lblVersion.Text);
                }
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
                lblVersion.Text = "未获取到最新的版本号";
                txtVersion.Text = "1.0.0";
            }
        }


        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            StringBuilder old = new StringBuilder(1024);
            foreach (var line in File.ReadLines(assemblyinfoPath))
            {
                old.AppendLine(line);
                CheckLine(sb, line);
            }
            File.WriteAllText(assemblyinfoPath, sb.ToString());
            txtMsg.AppendText("\r\n正在上传至服务器,完成后会自动关闭所有窗口,请耐心等待!");
            ProcessCmd();
            File.WriteAllText(assemblyinfoPath, old.ToString());
            this.Close();
            this.Dispose();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }


        /// <summary>
        /// 修改程序集版本号,描述,作者等数据
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="line"></param>
        private void CheckLine(StringBuilder sb, string line)
        {
            if (line.Contains("AssemblyDescription"))
            {
                sb.AppendLine("[assembly: AssemblyDescription(\"" + txtDescription.Text.Replace('\"', ' ').Replace('\\', ' ').Trim() + "\")]");
            }
            else if (line.Contains("AssemblyCompany"))
            {
                //当前windows登录用户名
                sb.AppendLine("[assembly: AssemblyCompany(\"" + Environment.UserName + "\")]");
            }
            else if (line.Contains("AssemblyCopyright"))
            {
                sb.AppendLine("[assembly: AssemblyCopyright(\"" + "Copyright © " + Environment.UserName + " " + DateTime.Now.ToString("yyyy-MM-dd") + "\")]");
            }
            else if (line.Contains("AssemblyVersion") && !line.Contains("*"))
            {
                sb.AppendLine("[assembly: AssemblyVersion(\"" + txtVersion.Text + "\")]");
            }
            else
            {
                sb.AppendLine(line);
            }
        }


        /// <summary>
        /// 执行 windows cmd 命令
        /// </summary>
        private void ProcessCmd()
        {

            Process proc = new Process();
            string strOuput = null;
            try
            {
                proc.StartInfo.FileName = "cmd.exe";

                //是否使用操作系统shell启动
                proc.StartInfo.UseShellExecute = false;

                // 接受来自调用程序的输入信息
                proc.StartInfo.RedirectStandardInput = true;

                //输出信息
                proc.StartInfo.RedirectStandardOutput = true;

                // 输出错误
                proc.StartInfo.RedirectStandardError = true;

                //不显示程序窗口
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();

                //构造命令
                StringBuilder sb = new StringBuilder();

                if (rdoNo.Checked)
                {
                    if (File.Exists($"{projectDir}packages.config"))
                    {
                        //因为不发布依赖项,所以需要移动 packages.config 文件
                        sb.Append($"move {projectDir}packages.config {toolPath}");
                        sb.Append("&&");
                    }

                    sb.Append(CreateCmd());

                    //还原 packages.config 文件
                    sb.Append($"&move {toolPath}\\packages.config {projectDir}");
                }
                else
                {
                    sb.Append(CreateCmd());
                }

                //退出
                sb.Append("&exit");

                //向cmd窗口发送输入信息
                proc.StandardInput.WriteLine(sb.ToString());
                proc.StandardInput.AutoFlush = true;

                strOuput = proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
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
        private string CreateCmd()
        {
            StringBuilder sb = new StringBuilder();

            //打包 dll 文件
            sb.Append($"nuget pack {projectFileName} -Build -Properties Configuration=Release -OutputDirectory {toolPath}");

            //发布 dll 文件
            sb.Append($"&&nuget push {toolPath}\\{targetName}.*.nupkg {pwd} -src {nugetUrl}/nuget");

            //删除生成的文件
            sb.Append($"&&del {toolPath}\\*.nupkg");

            return sb.ToString();
        }


        /// <summary>
        /// 请求api,获取当前使用的最高版本号
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        string HttpGet(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// 获取最高版本号
        /// </summary>
        /// <returns></returns>
        private string GetMaxVersion()
        {
            var version = string.Empty;
            var path = packagesUrl + targetName;
            if (Directory.Exists(path))
            {
                var dirs = Directory.GetDirectories(path);
                Dictionary<int[], string> dic = new Dictionary<int[], string>();
                foreach (var dir in dirs)
                {
                    var versionStr = dir.Split('\\').Last();//拿到版本号:1.0.1
                    var versionArray = ConvertVersionToIntArray(versionStr);//版本号转数组:int[]{1,0,1}
                    dic.Add(versionArray, versionStr);
                }
                version = GetMaxVersion(dic);
            }
            return version;
        }


        /// <summary>
        /// 计算最高版本号
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        string GetMaxVersion(Dictionary<int[], string> dic)
        {
            IGrouping<int, int[]> result = null;
            var keys = dic.Select(s => s.Key);
            for (int i = 0; i < keys.First().Length; i++)
            {
                result = Get(keys, i);
                keys = result;
            }
            return dic[result.First()];
        }

        IGrouping<int, int[]> Get(IEnumerable<int[]> keys, int index)
        {
            return keys.GroupBy(g => g[index]).OrderByDescending(o => o.Key).First();
        }
        

        /// <summary>
        /// 计算最新版本号 比如: 1.2.3 => 1.2.4
        /// </summary>
        /// <param name="nowVersion"></param>
        /// <returns></returns>
        string UpdateVersion(string nowVersion)
        {
            var nowVersionStrArray = nowVersion.Split('.');// 1 2 3
            var nowLastNum = nowVersionStrArray.Last();// 3
            var newLastNum = Convert.ToInt32(nowLastNum) + 1;// 4
            nowVersionStrArray[2] = newLastNum.ToString();// 1 2 4
            return nowVersionStrArray.Aggregate((a, s) => a += "." + s);//1.2.4
        }
        

        /// <summary>
        /// 把 1.0.1 转换成 int[]{1,0,1}
        /// </summary>
        /// <param name="str">版本号</param>
        /// <returns></returns>
        private int[] ConvertVersionToIntArray(string str)
        {
            var nums = str.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return Array.ConvertAll(nums, i => Convert.ToInt32(i));
        }


        /// <summary>
        /// 把 1.0.1 转换成 101
        /// </summary>
        /// <param name="str">版本号</param>
        /// <returns></returns>
        private string ConvertVersion(string str)
        {
            var nums = str.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var r = nums.Aggregate((a, t) => a += t);
            return r;
        }

    }
}
