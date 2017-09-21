using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.HTTP;
using Wangsu.WcsLib.Utility;

namespace Wangsu.WcsLib.Core
{
    /// <summary>
    /// 分片上传
    /// https://wcs.chinanetcenter.com/document/API/FileUpload/SliceUpload
    /// </summary>
    public class SliceUpload
    {
        public SliceUpload(Auth auth, Config config)
        {
            this.auth = auth;
            this.config = config;
            this.httpManager = new HttpManager();
            uploadBatch = Guid.NewGuid().ToString();
        }

        public SliceUpload(Mac mac, Config config) : this(new Auth(mac), config)
        {
        }

        /// <summary>
        /// 创建块(携带首片数据),同时检查CRC32
        /// </summary>
        /// <param name="uploadBatch">分片上传的会话 ID</param>
        /// <param name="blockSize">块大小，除了最后一块可能不足 4MB，前面的所有数据块恒定位 4MB</param>
        /// <param name="blockOrder">块块的顺序号，该序号从 0 开始指定</param>
        /// <param name="chunk">数据片，此操作都会携带第一个数据片</param>
        /// <param name="chunkSize">分片大小，一个块可以被分为若干片依次上传然后拼接或者不分片直接上传整块</param>
        /// <param name="uploadToken">上传凭证</param>
        /// <returns>此操作执行后的返回结果</returns>
        public HttpResult MakeBlock(long blockSize, long blockOrder, byte[] chunk, int chunkOffset, int chunkSize, string uploadToken, string key = null)
        {
            string url = config.GetUploadUrlPrefix() + "/mkblk/" + blockSize.ToString() + "/" + blockOrder.ToString();
            Dictionary<string, string> customHeaders = new Dictionary<string, string>();
            customHeaders.Add("UploadBatch", uploadBatch);
            if (!string.IsNullOrEmpty(key))
            {
                customHeaders.Add("Key", Base64.UrlSafeBase64Encode(key));
            }
            return httpManager.PostData(url, chunk, chunkOffset, chunkSize, uploadToken, customHeaders);
        }

        /// <summary>
        /// 上传数据片,同时检查CRC32
        /// </summary>
        /// <param name="uploadBatch">分片上传的会话 ID</param>
        /// <param name="chunk">数据片</param>
        /// <param name="offset">当前片在块中的偏移位置</param>
        /// <param name="chunkSize">当前片的大小</param>
        /// <param name="context">承接前一片数据用到的Context</param>
        /// <param name="uploadToken">上传凭证</param>
        /// <returns>此操作执行后的返回结果</returns>
        public HttpResult Bput(string context, long offset, byte[] chunk, int chunkOffset, int chunkSize, string uploadToken, string key = null)
        {
            string url = config.GetUploadUrlPrefix() + "/bput/" + context + "/" + offset.ToString();
            Dictionary<string, string> customHeaders = new Dictionary<string, string>();
            customHeaders.Add("UploadBatch", uploadBatch);
            if (!string.IsNullOrEmpty(key))
            {
                customHeaders.Add("Key", Base64.UrlSafeBase64Encode(key));
            }
            return httpManager.PostData(url, chunk, chunkOffset, chunkSize, uploadToken, customHeaders);
        }

        /// <summary>
        /// 根据已上传的所有分片数据创建文件
        /// </summary>
        /// <param name="uploadBatch">分片上传的会话 ID</param>
        /// <param name="size">文件大小</param>
        /// <param name="key">要保存的文件名</param>
        /// <param name="contexts">所有数据块的Context</param>
        /// <param name="uploadToken">上传凭证</param>
        /// <param name="putExtra">用户指定的额外参数</param>
        /// <returns>此操作执行后的返回结果</returns>
        public HttpResult MakeFile(long size, string key, string[] contexts, string uploadToken, PutExtra putExtra = null)
        {
            StringBuilder url = new StringBuilder();
            url.Append(config.GetUploadUrlPrefix() + "/mkfile/" + size.ToString());
            if (null != putExtra && null != putExtra.Params && putExtra.Params.Count > 0)
            {
                foreach (var p in putExtra.Params)
                {
                    if (p.Key.StartsWith("x:"))
                    {
                        url.Append("/" + p.Key + "/" + Base64.UrlSafeBase64Encode(p.Value));
                    }
                }
            }

            StringBuilder ctxList = new StringBuilder();
            foreach(var ctx in contexts)
            {
                ctxList.Append(ctx + ",");
            }
            Dictionary<string, string> customHeaders = new Dictionary<string, string>();
            customHeaders.Add("UploadBatch", uploadBatch);
            if (!string.IsNullOrEmpty(key))
            {
                customHeaders.Add("Key", Base64.UrlSafeBase64Encode(key));
            }
            if (null != putExtra && !string.IsNullOrEmpty(putExtra.MimeType))
            {
                customHeaders.Add("MimeType", putExtra.MimeType);
            }
            if (null != putExtra && 0 <= putExtra.Deadline)
            {
                customHeaders.Add("Deadline", putExtra.Deadline.ToString());
            }
            return httpManager.Post(url.ToString(), Encoding.UTF8.GetBytes(ctxList.ToString(0, ctxList.Length - 1)), uploadToken, "text/plain;charset=UTF-8", customHeaders);
        }

        private Auth auth;
        private Config config;
        private HttpManager httpManager;
        private string uploadBatch;
    }
}
