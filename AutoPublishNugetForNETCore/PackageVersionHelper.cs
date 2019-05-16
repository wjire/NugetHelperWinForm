using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace AutoPublishNugetForNETCore
{
    /// <summary>
    /// 
    /// </summary>
    public static class PackageVersionHelper
    {

        /// <summary>
        /// 获取Nuget服务器上的所有Package信息
        /// </summary>
        /// <returns></returns>
        public static string GetPackageInfo()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(AppConfigSetting.NugetUrl);
                string res = httpClient.GetStringAsync("/nuget/Packages").Result;
                return res;
            }
        }

        /// <summary>
        /// 获取本次发布应该使用的版本号
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static string GetNewVersion(string package)
        {
            Version maxVersion = new Version(1, 0, 0);
            string xmlString = GetPackageInfo();
            Dictionary<string, List<Version>> versionDic = GetPackageVersion(xmlString);
            if (versionDic.ContainsKey(package))
            {
                maxVersion = versionDic[package].Max();
            }
            return new Version(maxVersion.Major, maxVersion.Minor, maxVersion.Build + 1).ToString();
        }


        /// <summary>
        /// 解析Nuget服务器返回的所有Package信息,拿到Package名和其所有的版本号
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Version>> GetPackageVersion(string xmlString)
        {
            Dictionary<string, List<Version>> result = new Dictionary<string, List<Version>>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
            JObject jo = JObject.Parse(json);
            JArray array = (JArray)jo["feed"]["entry"];

            foreach (JToken item in array)
            {
                string name = item["title"]["#text"].ToString();
                string versionString = item["m:properties"]["d:Version"].ToString();
                Version version = new Version(versionString);
                if (result.ContainsKey(name))
                {
                    result[name].Add(version);
                }
                else
                {
                    result.Add(name, new List<Version> { version });
                }
            }
            return result;
        }
    }
}
