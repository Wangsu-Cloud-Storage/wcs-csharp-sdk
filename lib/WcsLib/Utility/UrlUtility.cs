using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Wangsu.WcsLib.Utility
{
    public sealed class UrlUtility
    {
        private static Regex reUrl = new Regex(@"(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
        private static Regex reUrlWithoutParameters = new Regex(@"(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,/~\+#]*)?");
        private static Regex reDirectoryUrl = new Regex(@"(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,/~\+#]*)?/");

        /// <summary>
        /// 是否合法URL
        /// </summary>
        /// <param name="url">待判断的 URL</param>
        /// <returns>真值</returns>
        public static bool IsValid(string url)
        {
            return reUrl.IsMatch(url);
        }

        /// <summary>
        /// 是否一般 URL（不判断参数）
        /// </summary>
        /// <param name="url">待判断的 URL</param>
        /// <returns>真值</returns>
        public static bool IsNormal(string url)
        {
            return reUrlWithoutParameters.IsMatch(url);
        }

        /// <summary>
        /// 是否 URL 目录
        /// </summary>
        /// <param name="url">待判断的 URL</param>
        /// <returns></returns>
        public static bool IsDirectory(string url)
        {
            return reDirectoryUrl.IsMatch(url);
        }

        /// <summary>
        /// 从原始URL转换为一般URL(根据需要截断)
        /// </summary>
        /// <param name="url">待转换的url</param>
        /// <returns></returns>
        public static string GetUrlWithoutParameters(string url)
        {
            return reUrlWithoutParameters.Match(url).Value;
        }

        /// <summary>
        /// URL分析，拆分出Host,Path,File,Query各个部分
        /// </summary>
        /// <param name="url">原始URL</param>
        /// <param name="host">host部分</param>
        /// <param name="path">path部分</param>
        /// <param name="file">文件名</param>
        /// <param name="query">参数</param>
        public static void UrlSplit(string url, out string host, out string path, out string file, out string query)
        {
            host = "";
            path = "";
            file = "";
            query = "";

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            int start = 0;

            try
            {
                Regex reHost = new Regex(@"(http|https):\/\/[\w\-_]+(\.[\w\-_]+)+");
                host = reHost.Match(url, start).Value;
                start += host.Length;

                Regex rePath = new Regex(@"/((\w|\-)*/)*");
                path = rePath.Match(url, start).Value;
                if (!string.IsNullOrEmpty(path))
                {
                    start += path.Length;
                }

                int index = url.IndexOf('?', start);
                if (index > 0)
                {
                    file = url.Substring(start, index - start);
                    query = url.Substring(index);
                }
                else
                {
                    file = url.Substring(start);
                }
            }
            catch (System.Exception)
            {
                // None
            }
        }

        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <returns>URL 编码字符串</returns>
        public static string UrlEncode(string text)
        {
            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// URL键值对编码
        /// </summary>
        /// <param name="values">键值对</param>
        /// <returns>URL编码的键值对数据</returns>
        public static string UrlFormEncode(Dictionary<string, string> values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in values)
            {
                sb.AppendFormat("{0}={1}&", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value));
            }
            // UMU: 有三种做法以上……还有人用 Mid……
            //string encodedString = sb.ToString();
            //return encodedString.Remove(encodedString.Length - 1);

            //sb.Remove(sb.Length - 1, 1);
            //return sb.ToString();
            return sb.ToString(0, sb.Length - 1);
        }

        /// <summary>
        /// URL 键值对编码，但不 Escape
        /// </summary>
        /// <param name="values">键值对</param>
        /// <returns>URL编码的键值对数据</returns>
        public static string MakeQueryString(Dictionary<string, string> values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in values)
            {
                sb.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
            }
            return sb.ToString(0, sb.Length - 1);
        }

        /// <summary>
        /// URL 键值对编码，但不 Escape
        /// </summary>
        /// <param name="values">键值对</param>
        /// <returns>URL编码的键值对数据</returns>
        public static byte[] MakeQueryData(Dictionary<string, string> values)
        {
            return Encoding.UTF8.GetBytes(MakeQueryString(values));
        }
    }
}
