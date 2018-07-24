using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetHelperWinForm
{
    /// <summary>
    /// 提供器抽象基类
    /// </summary>
    public abstract class BaseVersionProvider
    {

        /// <summary>
        /// 当前最高版本号
        /// </summary>
        public string MaxVersion => GetMaxVersion(Dirs);

        /// <summary>
        /// 即将发布的版本号
        /// </summary>
        public string NewVersion => CalculateNewVersion(MaxVersion);


        /// <summary>
        /// 版本号文件夹名集合
        /// </summary>
        protected IList<string> Dirs { get; }


        protected BaseVersionProvider(IList<string> dirs)
        {
            this.Dirs = dirs;
        }

        /// <summary>
        /// 获取当前最高版本号
        /// </summary>
        /// <param name="dirs"></param>
        /// <returns></returns>
        protected abstract string GetMaxVersion(IList<string> dirs);


        /// <summary>
        /// 计算即将发布的版本号
        /// </summary>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        protected abstract string CalculateNewVersion(string maxVersion);
    }
}
