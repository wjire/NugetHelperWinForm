using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetHelperWinForm
{

    /// <summary>
    /// 自定义方法实现版本号计算逻辑
    /// </summary>
    public class CustomProvider : BaseVersionProvider
    {

        public CustomProvider(IList<string> dirs) : base(dirs)
        {
        }

        protected override string GetMaxVersion(IList<string> dirs)
        {
            if (dirs == null || dirs.Count == 0) throw new ArgumentNullException(nameof(dirs));
            var dic = new Dictionary<int[], string>();
            foreach (var dir in dirs)
            {
                var versionStr = dir.Split('\\').Last();//拿到版本号,比如:"1.0.1"
                var versionArray = ConvertVersionToIntArray(versionStr);//版本号转数组:"1.0.1" => int[]{1,0,1}
                dic.Add(versionArray, versionStr);//将版本号作为键,版本号的字符串形式作为值存入字典
            }
            IGrouping<int, int[]> result = null;
            var keys = dic.Select(s => s.Key);
            for (int i = 0; i < keys.First().Length; i++)
            {
                result = GetMaxNumber(keys, i);
                keys = result;
            }
            return dic[result.First()];
        }
        
        protected override string CalculateNewVersion(string maxVersion)
        {
            var nowVersionStrArray = maxVersion.Split('.');//"1.2.3" => string[]{"1","2","3"}
            var nowLastNumStr = nowVersionStrArray.Last();//拿到 "3"
            var newLastNum = Convert.ToInt32(nowLastNumStr) + 1;//"3" => 4
            nowVersionStrArray[2] = newLastNum.ToString();//4=>"4"
            return nowVersionStrArray.Aggregate((a, s) => a += "." + s);// string[]{"1","2","4"} => "1.2.4"
        }


        /// <summary>
        /// 把 "1.0.1" 转换成 int[]{1,0,1}
        /// </summary>
        /// <param name="str">版本号</param>
        /// <returns></returns>
        private int[] ConvertVersionToIntArray(string str)
        {
            var nums = str.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return Array.ConvertAll(nums, Convert.ToInt32);
        }


        /// <summary>
        /// 递归获取最高版本号
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private IGrouping<int, int[]> GetMaxNumber(IEnumerable<int[]> keys, int index)
        {
            return keys.GroupBy(g => g[index]).OrderByDescending(o => o.Key).First();
        }
    }
}
