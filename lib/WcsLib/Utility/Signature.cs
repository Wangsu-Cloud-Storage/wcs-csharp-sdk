using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif
using System.Text;

namespace Wangsu.WcsLib.Utility
{
    public sealed class Signature
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mac">账号(密钥)</param>
        public Signature(Mac mac)
        {
            this.mac = mac;
        }

        /// <summary>
        /// 签名-字节数据
        /// </summary>
        /// <param name="data">待签名的字节</param>
        /// <returns>签名</returns>
        public string Sign(byte[] data)
        {
            // 仅是字符串拼接时，UMU 不喜欢用 Format，因为性能比较低；虽然它的可读性好一点点。
            //C#: string.Format("{0}:{1}", x, y);
            //Python: '{0}:{1}'.format(x, y)
            return mac.AccessKey + ":" + EncodeSign(data);
        }

        /// <summary>
        /// 签名-字符串数据
        /// </summary>
        /// <param name="str">待签名的字符串</param>
        /// <returns>签名</returns>
        public string Sign(string str)
        {
            return Sign(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 附带数据的签名
        /// </summary>
        /// <param name="data">待签名的字节</param>
        /// <returns>签名</returns>
        public string SignWithData(byte[] data)
        {
            string sstr = Base64.UrlSafeBase64Encode(data);
            return mac.AccessKey + ":" + EncodeSign(sstr) + ":" + sstr;
        }

        /// <summary>
        /// 附带数据的签名
        /// 可以用此函数来生成上传凭证(MakeUploadToken)
        /// https://wcs.chinanetcenter.com/document/API/Token/UploadToken
        /// </summary>
        /// <param name="str">待签名的字符串</param>
        /// <returns>签名</returns>
        public string SignWithData(string str)
        {
            return SignWithData(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// HTTP 请求签名
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns>签名</returns>
        public string SignRequest(string url, byte[] body = null)
        {
            Uri u = new Uri(url);
            byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes(u.PathAndQuery);
            // To dispose of it indirectly
            using (MemoryStream buffer = new MemoryStream())
            {
                buffer.Write(pathAndQueryBytes, 0, pathAndQueryBytes.Length);
                buffer.WriteByte((byte)'\n');
                if (body != null && body.Length > 0)
                {
                    buffer.Write(body, 0, body.Length);
                }
                return Sign(buffer.ToArray());
            }
        }

        /// <summary>
        /// HTTP 请求签名
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns>签名</returns>
        public string SignRequest(string url, string body)
        {
            return SignRequest(url, Encoding.UTF8.GetBytes(body));
        }

        #region Private
        private string EncodeSign(byte[] data)
        {
#if WINDOWS_UWP
            var hmac = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            var skBuffer = CryptographicBuffer.ConvertStringToBinary(mac.SecretKey, BinaryStringEncoding.Utf8);
            var hmacKey = hmac.CreateKey(skBuffer);
            var dataBuffer = CryptographicBuffer.CreateFromByteArray(data);
            var signBuffer = CryptographicEngine.Sign(hmacKey, dataBuffer);
            byte[] hashData;
            CryptographicBuffer.CopyToByteArray(signBuffer, out hashData);
#else
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey));
            byte[] hashData = hmac.ComputeHash(data);
#endif
            //return Base64.UrlSafeBase64Encode(hashData);    // Qiniu
            return Base64.UrlSafeBase64Encode(Hash.ToHexString(hashData));  // Wangsu
        }

        private string EncodeSign(string str)
        {
            return EncodeSign(Encoding.UTF8.GetBytes(str));
        }

        private Mac mac;
        #endregion
    }
}
