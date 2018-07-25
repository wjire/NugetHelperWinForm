using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetHelperWinForm
{

    public class EmptyProvider : BaseVersionProvider
    {

        public EmptyProvider()
        {
            
        }
        public EmptyProvider(IList<string> dirs) : base(dirs)
        {
        }

        protected override string GetMaxVersion(IList<string> dirs)
        {
            return "第一次上传该项目";
        }

        protected override string CalculateNewVersion(string maxVersion)
        {
            return "1.0.0";
        }
    }
}
