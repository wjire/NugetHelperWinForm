using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetHelperWinForm
{
    /// <summary>
    /// 利用 DoNet 自带 Version 类实现版本号计算逻辑
    /// </summary>
    public class DotNetProvider : BaseVersionProvider
    {

        public DotNetProvider(IList<string> dirs) : base(dirs)
        {

        }

        protected override string GetMaxVersion(IList<string> dirs)
        {
            if (dirs == null || dirs.Count == 0) throw new ArgumentNullException(nameof(dirs));
            var versionList = dirs.Select(dir => new Version(dir.Split('\\').Last())).ToList();
            return versionList.Max().ToString();
        }


        protected override string CalculateNewVersion(string maxVersion)
        {
            if (string.IsNullOrWhiteSpace(maxVersion)) throw new ArgumentNullException(nameof(maxVersion));
            var max = new Version(maxVersion);
            var res = string.Empty;
            if (max.Revision >= 0) res = new Version(max.Major, max.Minor, max.Build, max.Revision + 1).ToString();
            else if (max.Build >= 0) res = new Version(max.Major, max.Minor, max.Build + 1).ToString();
            else if (max.Minor >= 0) res = new Version(max.Major, max.Minor + 1).ToString();
            else throw new InvalidCastException("版本号不符合要求");
            return res;
        }
    }
}
