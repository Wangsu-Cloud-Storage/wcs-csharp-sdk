using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.Utility;

namespace Wangsu.WcsLib.Core
{
    public class Config
    {
        public Config(string uploadHost = null, string manageHost = null, bool useHttp = false)
        {
            // set default
            UploadHost = null == uploadHost ? "apitestuser.up0.v1.wcsapi.com" : uploadHost;
            ManageHost = null == manageHost ? "apitestuser.mgr0.v1.wcsapi.com" : manageHost;
            UseHttps = useHttp;
        }

        /// <summary>
        /// 获取资源管理域名
        /// </summary>
        /// <returns></returns>
        public string GetManageUrlPrefix()
        {
            return (UseHttps ? "https://" : "http://") + ManageHost;
        }

        /// <summary>
        /// 获取资源管理域名
        /// </summary>
        /// <returns></returns>
        public string GetUploadUrlPrefix()
        {
            return (UseHttps ? "https://" : "http://") + UploadHost;
        }

        /// <summary>
        /// 上传域名
        /// </summary>
        public string UploadHost { set; get; }

        /// <summary>
        /// 管理域名
        /// </summary>
        public string ManageHost { set; get; }

        /// <summary>
        /// 是否采用 HTTPS 域名
        /// </summary>
        public bool UseHttps { set; get; }

        public static long BLOCK_SIZE = 4 * 1024 * 1024;
    }
}
