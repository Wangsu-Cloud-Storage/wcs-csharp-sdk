using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.Utility;

namespace Wangsu.WcsLib.HTTP
{
    /// <summary>
    /// HttpManager for .NET
    /// 不支持 UWP
    /// </summary>
    public sealed class HttpManager
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="allowAutoRedirect">是否允许 HttpWebRequest 的重定向，默认禁止</param>
        public HttpManager(bool allowAutoRedirect = false)
        {
            this.allowAutoRedirect = allowAutoRedirect;
            userAgent = GetDefaultUserAgent();
        }

        /// <summary>
        /// UserAgent 属性
        /// </summary>
        public string UserAgent
        {
            get
            {
                return userAgent;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                userAgent = value;
            }
        }

        /// <summary>
        /// 获取默认 UserAgent
        /// </summary>
        /// <returns>默认 UserAgent</returns>
        public static string GetDefaultUserAgent()
        {
            // UMU:
            // GetCallingAssembly() -> EXE
            // GetExecutingAssembly() -> WcsLib
            AssemblyInfo info = new AssemblyInfo(Assembly.GetExecutingAssembly());
            return string.Format("WCS-C#-SDK-{0}(https://wcs.chinanetcenter.com)", info.Version);
            //return string.Format("{0}/{1} ({2}; {3}; {4})", info.Product, info.Version, info.Title, Environment.OSVersion.Platform, Environment.OSVersion.Version);
        }

        /// <summary>
        /// 多部分表单数据(multi-part form-data)的分界(boundary)标识
        /// </summary>
        /// <returns>分界(boundary)标识字符串</returns>
        public static string CreateFormDataBoundary()
        {
            return "WcsLibBoundary" + DateTime.UtcNow.Ticks.ToString("X16");
        }

        public HttpResult Get(string url, string token = null, string mimeType = null, bool binaryMode = false)
        {
            HttpResult result = new HttpResult();
            HttpWebRequest webRequest = null;

            try
            {
                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Method = "GET";
                if (!string.IsNullOrEmpty(mimeType))
                {
                    webRequest.ContentType = mimeType;
                }

                webRequest.UserAgent = userAgent;
                webRequest.AllowAutoRedirect = allowAutoRedirect;
                webRequest.ServicePoint.Expect100Continue = false;

                if (!string.IsNullOrEmpty(token))
                {
                    webRequest.Headers.Add("Authorization", token);
                }

                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                if (null != webResponse)
                {
                    result.Code = (int)webResponse.StatusCode;
                    result.UserDefinedCode = (int)webResponse.StatusCode;

                    GetHeaders(webResponse, ref result);

                    if (binaryMode)
                    {
                        int length = (int)webResponse.ContentLength;
                        result.Data = new byte[length];
                        int bytesLeft = length;
                        int bytesRead = 0;

                        using (BinaryReader br = new BinaryReader(webResponse.GetResponseStream()))
                        {
                            while (bytesLeft > 0)
                            {
                                bytesRead = br.Read(result.Data, length - bytesLeft, bytesLeft);
                                bytesLeft -= bytesRead;
                            }
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            result.Text = sr.ReadToEnd();
                        }
                    }

                    webResponse.Close();
                }
            }
            catch (WebException webException)
            {
                HttpWebResponse webExceptionResponse = webException.Response as HttpWebResponse;
                if (null != webExceptionResponse)
                {
                    result.Code = (int)webExceptionResponse.StatusCode;
                    result.UserDefinedCode = (int)webExceptionResponse.StatusCode;

                    GetHeaders(webExceptionResponse, ref result);

                    using (StreamReader sr = new StreamReader(webExceptionResponse.GetResponseStream()))
                    {
                        result.Text = sr.ReadToEnd();
                    }

                    webExceptionResponse.Close();
                }
                else
                {
                    result.UserDefinedCode = (int)webException.Status;
                    result.UserDefinedText = string.Format("[{0}] [{1}] [HTTP-GET] Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), userAgent) + webException.Message;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] [{1}] [HTTP-GET] Error:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), userAgent);
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(" " + e.Message);
                    e = e.InnerException;
                }
                sb.AppendLine();

                result.UserDefinedCode = ex.HResult;
                result.UserDefinedText += sb.ToString();
            }
            finally
            {
                if (null != webRequest)
                {
                    webRequest.Abort();
                }
            }

            return result;
        }

        public HttpResult Post(string url, byte[] data, int dataOffset, int dataSize, string token = null, string mimeType = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            HttpResult result = new HttpResult();
            HttpWebRequest webRequest = null;

            try
            {
                webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Method = "POST";
                if (!string.IsNullOrEmpty(mimeType))
                {
                    webRequest.ContentType = mimeType;
                }
                
                webRequest.UserAgent = userAgent;
                webRequest.AllowAutoRedirect = allowAutoRedirect;
                webRequest.ServicePoint.Expect100Continue = false;

                if (!string.IsNullOrEmpty(token))
                {
                    webRequest.Headers.Add("Authorization", token);
                }

                if (null != customHeaders)
                {
                    foreach (var header in customHeaders)
                    {
                        webRequest.Headers.Add(header.Key, header.Value);
                    }
                }

                if (data != null)
                {
#if DEBUG
                    if (dataSize < 0 || dataOffset < 0 || dataOffset + dataSize > data.Length)
                    {
                        throw new ArgumentOutOfRangeException("dataSize");
                    }
#endif
                    if (dataSize > 0)
                    {
                        // UMU: 下面这句非必要
                        webRequest.ContentLength = dataSize;
#if DEBUG
                        Console.WriteLine("Post {0} bytes.", dataSize);
#endif
                        webRequest.AllowWriteStreamBuffering = true;
                        using (Stream stream = webRequest.GetRequestStream())
                        {
                            stream.Write(data, dataOffset, dataSize);
                            stream.Flush();
                        }
                    }
                }

                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                if (null != webResponse)
                {
                    result.Code = (int)webResponse.StatusCode;
                    result.UserDefinedCode = (int)webResponse.StatusCode;

                    GetHeaders(webResponse, ref result);

                    if (binaryMode)
                    {
                        int length = (int)webResponse.ContentLength;
                        result.Data = new byte[length];
                        int bytesLeft = length;
                        int bytesRead = 0;

                        using (BinaryReader br = new BinaryReader(webResponse.GetResponseStream()))
                        {
                            while (bytesLeft > 0)
                            {
                                bytesRead = br.Read(result.Data, length - bytesLeft, bytesLeft);
                                bytesLeft -= bytesRead;
                            }
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            result.Text = sr.ReadToEnd();
                        }
                    }

                    webResponse.Close();
                }
            }
            catch (WebException webException)
            {
                HttpWebResponse webExceptionResponse = webException.Response as HttpWebResponse;
                if (null != webExceptionResponse)
                {
                    result.Code = (int)webExceptionResponse.StatusCode;
                    result.UserDefinedCode = (int)webExceptionResponse.StatusCode;

                    GetHeaders(webExceptionResponse, ref result);

                    using (StreamReader sr = new StreamReader(webExceptionResponse.GetResponseStream()))
                    {
                        result.Text = sr.ReadToEnd();
                    }

                    webExceptionResponse.Close();
                }
                else
                {
                    result.UserDefinedCode = (int)webException.Status;
                    result.UserDefinedText = string.Format("[{0}] [{1}] [HTTP-POST] Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), userAgent) + webException.Message;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] [{1}] [HTTP-POST] Error:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), userAgent);
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(" " + e.Message);
                    e = e.InnerException;
                }
                sb.AppendLine();

                result.UserDefinedCode = ex.HResult;
                result.UserDefinedText += sb.ToString();
            }
            finally
            {
                if (null != webRequest)
                {
                    webRequest.Abort();
                }
            }

            return result;
        }

        public HttpResult Post(string url, byte[] data, string token = null, string mimeType = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, data, 0, data.Length, token, mimeType, customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST 方法(包含表单数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">表单数据</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult PostForm(string url, byte[] data, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, data, token, "application/x-www-form-urlencoded", customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含 JSON 文本的 body 数据)
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="data">主体数据(JSON 文本)</param>
        /// <param name="token">令牌(凭证)[可选]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult PostJson(string url, string data, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, Encoding.UTF8.GetBytes(data), token, "application/json", customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST 方法(包含多分部数据, multipart/form-data)
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="data">主体数据</param>
        /// <param name="boundary">分界标志</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult PostMultipart(string url, byte[] data, string boundary, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, data, token, string.Format("multipart/form-data; boundary={0}", boundary), customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(字节数据)</param>
        /// <param name="dataSize">主体数据长度</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostData(string url, byte[] data, int dataOffset, int dataSize, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, data, dataOffset, dataSize, token, "application/octet-stream", customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(字节数据)</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostData(string url, byte[] data, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return PostData(url, data, 0, data.Length, token, customHeaders, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含普通文本的 body 数据)
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="data">主体数据(普通文本)</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult PostText(string url, string data, string token = null, Dictionary<string, string> customHeaders = null, bool binaryMode = false)
        {
            return Post(url, Encoding.UTF8.GetBytes(data), token, "text/plain", customHeaders, binaryMode);
        }

        /// <summary>
        /// 获取返回信息头
        /// </summary>
        /// <param name="webResponse">正在被读取的 HTTP 响应</param>
        /// <param name="result">ResponseHeaders 被填充</param>
        private void GetHeaders(HttpWebResponse webResponse, ref HttpResult result)
        {
            if (null != webResponse)
            {
                if (null == result.ResponseHeaders)
                {
                    result.ResponseHeaders = new Dictionary<string, string>();
                }

                result.ResponseHeaders.Add("ProtocolVersion", webResponse.ProtocolVersion.ToString());

                if (!string.IsNullOrEmpty(webResponse.CharacterSet))
                {
                    result.ResponseHeaders.Add("Characterset", webResponse.CharacterSet);
                }

                if (!string.IsNullOrEmpty(webResponse.ContentEncoding))
                {
                    result.ResponseHeaders.Add("ContentEncoding", webResponse.ContentEncoding);
                }

                if (!string.IsNullOrEmpty(webResponse.ContentType))
                {
                    result.ResponseHeaders.Add("ContentType", webResponse.ContentType);
                }

                result.ResponseHeaders.Add("ContentLength", webResponse.ContentLength.ToString());

                var headers = webResponse.Headers;
                if (null != headers && headers.Count > 0)
                {
                    foreach (var key in headers.AllKeys)
                    {
                        result.ResponseHeaders.Add(key, headers[key]);
                    }
                }
            }
        }

        private bool allowAutoRedirect;
        private string userAgent;
    }
}
