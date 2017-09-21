using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wangsu.WcsLib.HTTP
{
    public class HttpResult
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public HttpResult()
        {
            Code = USER_DEFINED_CODE;
            Text = null;
            Data = null;
            ResponseHeaders = null;

            UserDefinedCode = USER_DEFINED_CODE;
        }

        /// <summary>
        /// 对象复制
        /// </summary>
        /// <param name="hr">要复制其内容的来源</param>
        public void Copy(HttpResult source)
        {
            this.Code = source.Code;
            this.Text = source.Text;
            this.Data = source.Data;
            this.ResponseHeaders = source.ResponseHeaders;

            this.UserDefinedCode = source.UserDefinedCode;
            this.UserDefinedText = source.UserDefinedText;
        }

        /// <summary>
        /// 转换为易读或便于打印的字符串格式
        /// </summary>
        /// <returns>便于打印和阅读的字符串</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("code:{0}", Code);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(Text))
            {
                sb.AppendLine("text:");
                sb.AppendLine(Text);
            }

            if (Data != null)
            {
                sb.AppendLine("data:");
                int maxLength = 1024;
                if (Data.Length <= maxLength)
                {
                    sb.AppendLine(Encoding.UTF8.GetString(Data));
                }
                else
                {
                    sb.AppendLine(Encoding.UTF8.GetString(Data, 0, maxLength));
                    sb.AppendFormat("... Total {0} Bytes", Data.Length);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendFormat("user-defined-code:{0}", UserDefinedCode);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(UserDefinedText))
            {
                sb.AppendLine("user-defined-text:");
                sb.AppendLine(UserDefinedText);
            }

            if (ResponseHeaders != null)
            {
                sb.AppendLine("user-defined-headers:");
                foreach (var d in ResponseHeaders)
                {
                    sb.AppendLine(string.Format("{0}:{1}", d.Key, d.Value));
                }
            }

            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// HTTP 状态码
        /// eg: System.Net.HttpStatusCode.OK
        /// https://wcs.chinanetcenter.com/document/API/Appendix/StatusCode
        /// </summary>
        public int Code { set; get; }

        /// <summary>
        /// 消息或错误文本
        /// </summary>
        public string Text { set; get; }

        /// <summary>
        /// 消息或错误(二进制格式)
        /// </summary>
        public byte[] Data { set; get; }

        /// <summary>
        /// 响应头
        /// https://wcs.chinanetcenter.com/document/API/Appendix/HTTPExtentHeader
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { set; get; }

        /// <summary>
        /// 用户自定义代码
        /// </summary>
        public int UserDefinedCode { set; get; }

        /// <summary>
        /// 用户自定义信息
        /// </summary>
        public string UserDefinedText { set; get; }
        
        public const int USER_DEFINED_CODE = 0;
    }
}
