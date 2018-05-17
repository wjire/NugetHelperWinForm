using System;
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
                string version = HttpGet(url).Trim('"');
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
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadLines(assemblyinfoPath))
            {
                CheckLine(sb, line);
            }
            File.WriteAllText(assemblyinfoPath, sb.ToString());
            txtMsg.AppendText("\r\n正在上传至服务器,完成后会自动关闭所有窗口,请耐心等待!");
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
