using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetHelperWinForm
{
    public class ErrorProvider : BaseVersionProvider
    {
        public ErrorProvider()
        {
            
        }

        public ErrorProvider(IList<string> dirs) : base(dirs)
        {
        }

        protected override string GetMaxVersion(IList<string> dirs)
        {
            return "程序出现异常,未获取到最新的版本号";
        }

        protected override string CalculateNewVersion(string maxVersion)
        {
            return "1.0.0";
        }
    }
}
