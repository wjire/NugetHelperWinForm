using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NugetHelperWinForm
{
    public partial class Form1 : Form
    {
        //AssemblyInfo.cs文件的绝对路径
        private readonly string filePath;

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

        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string[] args)
        {
            InitializeComponent();

            projectFileName = args[0];
            targetName = args[1];
            filePath = args[2] + @"Properties\AssemblyInfo.cs";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string url = nugetUrl + apiUri + targetName;
            lblName.Text = targetName;
            txtDescription.Text = url;
            var version = HttpGet(url).Trim('"');
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

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadLines(filePath))
            {
                CheckLine(sb, line);
            }
            File.WriteAllText(filePath, sb.ToString());
            ProcessCmd();
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
        void CheckLine(StringBuilder sb, string line)
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
        /// 执行 windows 批处理文件
        /// </summary>
        private void ProcessCmd()
        {
            Process proc = new Process();
            try
            {
                //设置批处理文件的路径
                proc.StartInfo.FileName = toolPath + @"\NugetHelper.bat";

                //设置批处理文件的参数
                string[] args = new string[]
                {
                    projectFileName,
                    targetName,
                    toolPath,
                    nugetUrl + "/nuget",
                    pwd
                };

                //在 cmd 命令提示符下运行程序,多个参数以 空格 隔开
                proc.StartInfo.Arguments = args.Aggregate((a, s) => a += " " + s);

                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occurred :{ex.Message},{ex.StackTrace}");
            }
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
    }
}
