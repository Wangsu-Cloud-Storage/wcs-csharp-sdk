using System;
using System.Collections.Generic;

namespace Wangsu.WcsLib.Core
{
    /// <summary>
    /// 文件上传的额外可选设置
    /// </summary>
    public class PutExtra
    {
        public PutExtra()
        {
            Deadline = -1;
        }

        /// <summary>
        /// 上传可选参数字典，参数名次以 x: 开头
        /// </summary>
        public Dictionary<string, string> Params;

        /// <summary>
        /// 指定文件的 MimeType
        /// </summary>
        public string MimeType { set; get; }

        /// <summary>
        /// 文件保存期限。超过保存天数文件自动删除,单位：天。例如：1、2、3…… 
        /// 注：0表示尽快删除，上传文件时建议不配置为0
        /// </summary>
        public int Deadline { set; get; }
    }
}
