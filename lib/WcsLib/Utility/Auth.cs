using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wangsu.WcsLib.Utility
{
    /// <summary>
    /// Authentication
    /// </summary>
    public sealed class Auth
    {
        /// <summary>
        /// 一般初始化
        /// </summary>
        /// <param name="mac">账号(密钥)</param>
        public Auth(Mac mac)
        {
            signature = new Signature(mac);
        }

        /// <summary>
        /// 生成管理凭证
        /// https://wcs.chinanetcenter.com/document/API/Token/AccessToken
        /// https://wcs.chinanetcenter.com/document/Tools/GenerateManageToken
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="body">请求的主体内容</param>
        /// <returns>生成的管理凭证</returns>
        public string CreateManageToken(string url, byte[] body)
        {
            return signature.SignRequest(url, body);
        }

        /// <summary>
        /// 生成管理凭证
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="body">请求的主体内容</param>
        /// <returns>生成的管理凭证</returns>
        public string CreateManageToken(string url, string body)
        {
            return signature.SignRequest(url, body);
        }

        /// <summary>
        /// 生成管理凭证-不包含body
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <returns>生成的管理凭证</returns>
        public string CreateManageToken(string url)
        {
            return signature.SignRequest(url);
        }

        /// <summary>
        /// 生成上传凭证
        /// https://wcs.chinanetcenter.com/document/API/Token/UploadToken
        /// https://wcs.chinanetcenter.com/document/Tools/GenerateUploadToken
        /// </summary>
        /// <param name="putPolicy">上传策略，JSON 字符串</param>
        /// <returns>上传凭证</returns>
        public string CreateUploadToken(string putPolicy)
        {
            return signature.SignWithData(putPolicy);
        }

        private Signature signature;
    }
}
