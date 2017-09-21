using System;
using System.Text;

namespace Wangsu.WcsLib.Utility
{
    /// <summary>
    /// Base64 编码/解码
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// 字符串 URL 安全 Base64 编码
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <returns>编码</returns>
        public static string UrlSafeBase64Encode(string text)
        {
            return UrlSafeBase64Encode(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// URL 安全的 Base64 编码
        /// </summary>
        /// <param name="data">需要编码的字节数据</param>
        /// <returns>编码</returns>
        public static string UrlSafeBase64Encode(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// bucket:key 编码
        /// == Python SDK: def entry(bucket, key)
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件 Key</param>
        /// <returns>编码</returns>
        public static string UrlSafeBase64Encode(string bucket, string key)
        {
            return UrlSafeBase64Encode(bucket + ":" + key);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="text">待解码的字符串</param>
        /// <returns>已解码字节</returns>
        public static byte[] UrlSafeBase64DecodeByte(string text)
        {
            return Convert.FromBase64String(text.Replace('-', '+').Replace('_', '/'));
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="text">待解码的字符串</param>
        /// <returns>已解码字符串</returns>
        public static string UrlSafeBase64Decode(string text)
        {
            return Encoding.UTF8.GetString(UrlSafeBase64DecodeByte(text));
        }
    }
}
