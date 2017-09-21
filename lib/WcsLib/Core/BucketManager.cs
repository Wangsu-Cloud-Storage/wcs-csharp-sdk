using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.HTTP;
using Wangsu.WcsLib.Utility;

namespace Wangsu.WcsLib.Core
{
    public class BucketManager
    {
        /// <summary>
        /// 构造 BucketManager
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="config"></param>
        public BucketManager(Auth auth, Config config)
        {
            this.auth = auth;
            this.httpManager = new HttpManager();
            this.config = config;
        }

        /// <summary>
        /// 构造 BucketManager
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="config"></param>
        public BucketManager(Mac mac, Config config)
        {
            this.auth = new Auth(mac);
            this.httpManager = new HttpManager();
            this.config = config;
        }

        #region Query

        /// <summary>
        /// 获取空间(bucket)列表
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/listbucket
        /// </summary>
        /// <returns>空间列表</returns>
        public HttpResult Buckets()
        {
            // 蛋疼的测试
            //string url = config.GetManageUrlPrefix() + "/bucket/list?name=UMU";
            string url = config.GetManageUrlPrefix() + "/bucket/list";
            string token = auth.CreateManageToken(url);
            return httpManager.Get(url, token);
        }

        /// <summary>
        /// 获取文件列表
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/list
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="prefix">可选，文件名前缀</param>
        /// <param name="marker">可选，上次列举返回的位置标记，作为本次列举的起点信息。</param>
        /// <param name="limit">可选，列举条目数，范围1-1000。默认值为1000。</param>
        /// <param name="mode">可选，指定列表排序方式：0代表优先列出目录下的文件；1代表优先列出目录下的文件夹。不指定该参数时，即按照 key 排序列出目录下的所有文件及子目录下的文件。</param>
        /// <returns>文件列表</returns>
        public HttpResult BucketList(string bucket, int limit = 0, string prefix = null, int mode = -1, string marker = null)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
#endif
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("bucket", bucket);
            if (1 <= limit && limit <= 1000)
            {
                query.Add("limit", limit.ToString());
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                query.Add("prefix", Base64.UrlSafeBase64Encode(prefix));
            }
            if (0 == mode || 1 == mode)
            {
                query.Add("mode", mode.ToString());
            }
            if (!string.IsNullOrEmpty(marker))
            {
                query.Add("marker", marker);
            }

            string url = config.GetManageUrlPrefix() + "/list?" + UrlUtility.MakeQueryString(query);
            string token = auth.CreateManageToken(url);
            return httpManager.Get(url, token);
        }

        /// <summary>
        /// 获取空间存储量(bucket stat)
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/bucketstat
        /// </summary>
        /// <param name="name">空间名称列表，格式为: "<bucket_name1>|<bucket_name2>|……"</param>
        /// <param name="startdate">统计开始时间，格式为: "yyyy-mm-dd"</param>
        /// <param name="enddate">空间名称列表，格式为: "yyyy-mm-dd"，注：查询的时间跨度最长为六个月</param>
        /// <returns>存储量信息</returns>
        public HttpResult BucketStat(string name, string startdate, string enddate)
        {
#if DEBUG
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(startdate))
            {
                throw new ArgumentNullException("startdate");
            }
            if (string.IsNullOrEmpty(enddate))
            {
                throw new ArgumentNullException("enddate");
            }
#endif
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("name", Base64.UrlSafeBase64Encode(name));
            query.Add("startdate", startdate);
            query.Add("enddate", enddate);

            string url = config.GetManageUrlPrefix() + "/bucket/stat?" + UrlUtility.MakeQueryString(query);
            string token = auth.CreateManageToken(url);
            return httpManager.Get(url, token);
        }

        /// <summary>
        /// 获取空间文件信息
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/stat
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件 key</param>
        /// <returns>文件信息</returns>
        public HttpResult Stat(string bucket, string key)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
#endif
            // 仅是字符串拼接时，UMU 不喜欢用 Format，因为性能比较低；虽然它的可读性好一点点。
            //string url = string.Format("{0}{1}", config.GetManageUrlPrefix(), StatOp(bucket, key));
            string url = config.GetManageUrlPrefix() + "/stat/" + Base64.UrlSafeBase64Encode(bucket, key);
            string token = auth.CreateManageToken(url);
            return httpManager.Get(url, token);
        }

        /// <summary>
        /// 获取图片基本信息
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/imageInfo
        /// </summary>
        /// <param name="imageUrl">图片 URL，可以是云存储的二级域名，或者空间绑定的域名。</param>
        /// <returns>图片的基本信息，包括图片的大小，宽度，高度，色彩空间，不含拍摄信息。</returns>
        public HttpResult ImageInfo(string imageUrl)
        {
#if DEBUG
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException("imageUrl");
            }
#endif
            // 不需要 token
            return httpManager.Get(imageUrl + "?op=imageInfo");
        }

        /// <summary>
        /// 获取图片EXIF信息
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/exif
        /// </summary>
        /// <param name="imageUrl">图片 URL，可以是云存储的二级域名，或者空间绑定的域名。</param>
        /// <returns>图片的详细信息（包括图像信息与拍摄信息）。</returns>
        public HttpResult Exif(string imageUrl)
        {
#if DEBUG
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException("imageUrl");
            }
#endif
            // 不需要 token
            return httpManager.Get(imageUrl + "?op=exif");
        }

        /// <summary>
        /// 音视频元数据(avinfo)
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/avinfo
        /// </summary>
        /// <param name="avUrl">音视频 URL，可以是云存储的二级域名，或者空间绑定的域名。</param>
        /// <returns>音视频资源的元信息</returns>
        public HttpResult AvInfo(string avUrl)
        {
#if DEBUG
            if (string.IsNullOrEmpty(avUrl))
            {
                throw new ArgumentNullException("avUrl");
            }
#endif
            // 不需要 token
            return httpManager.Get(avUrl + "?op=avinfo");
        }

        /// <summary>
        /// 音视频简单元数据(avinfo2)
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/avinfo
        /// </summary>
        /// <param name="avUrl">音视频 URL，可以是云存储的二级域名，或者空间绑定的域名。</param>
        /// <returns>音视频资源的简单元信息</returns>
        public HttpResult AvInfo2(string avUrl)
        {
#if DEBUG
            if (string.IsNullOrEmpty(avUrl))
            {
                throw new ArgumentNullException("avUrl");
            }
#endif
            // 不需要 token
            return httpManager.Get(avUrl + "?op=avinfo2");
        }

        /// <summary>
        /// 查询持久化处理状态
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/PersistentStatus
        /// </summary>
        /// <param name="persistentId">上传预处理或者触发持久化处理接口返回的persistentId</param>
        /// <returns>持久化处理的状态</returns>
        public HttpResult PersistentStatus(string persistentId)
        {
#if DEBUG
            if (string.IsNullOrEmpty(persistentId))
            {
                throw new ArgumentNullException("persistentId");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/status/get/prefop?persistentId=" + persistentId;
            // 不需要 token
            return httpManager.Get(url);
        }

        #endregion Query

        #region Change

        /// <summary>
        /// 删除文件
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/delete
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件 key</param>
        /// <returns>删除结果</returns>
        public HttpResult Delete(string bucket, string key)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/delete/" + Base64.UrlSafeBase64Encode(bucket, key);
            string token = auth.CreateManageToken(url);
            return httpManager.Post(url, null, token);
        }

        /// <summary>
        /// 更新镜像资源
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/prefetch
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件 key</param>
        /// <returns>更新结果</returns>
        public HttpResult Prefetch(string bucket, string[] keys)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (null == keys || 0 == keys.Length)
            {
                throw new ArgumentNullException("keys");
            }
#endif
            StringBuilder sb = new StringBuilder();
            foreach (string key in keys)
            {
                sb.Append(key + '|');
            }
            string keysString = sb.ToString(0, sb.Length - 1);
            string url = config.GetManageUrlPrefix() + "/prefetch/" + Base64.UrlSafeBase64Encode(bucket, keysString);
            string token = auth.CreateManageToken(url);
            return httpManager.Post(url, null, token);
        }

        /// <summary>
        /// 移动资源(move)
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/move
        /// </summary>
        /// <param name="sourceBucket">空间名称</param>
        /// <param name="sourceKey">文件 key</param>
        /// <param name="destinationBucket">空间名称</param>
        /// <param name="destinationKey">文件 key</param>
        /// <returns>移动结果，如果目标空间存在同名资源，不会覆盖。</returns>
        public HttpResult Move(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey)
        {
#if DEBUG
            if (string.IsNullOrEmpty(sourceBucket))
            {
                throw new ArgumentNullException("sourceBucket");
            }
            if (string.IsNullOrEmpty(sourceKey))
            {
                throw new ArgumentNullException("sourceKey");
            }
            if (string.IsNullOrEmpty(destinationBucket))
            {
                throw new ArgumentNullException("destinationBucket");
            }
            if (string.IsNullOrEmpty(destinationKey))
            {
                throw new ArgumentNullException("destinationKey");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/move/" + Base64.UrlSafeBase64Encode(sourceBucket, sourceKey) + "/" + Base64.UrlSafeBase64Encode(destinationBucket, destinationKey);
            string token = auth.CreateManageToken(url);
            return httpManager.Post(url, null, token);
        }

        /// <summary>
        /// 复制资源(copy)
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/copy
        /// </summary>
        /// <param name="sourceBucket">空间名称</param>
        /// <param name="sourceKey">文件 key</param>
        /// <param name="destinationBucket">空间名称</param>
        /// <param name="destinationKey">文件 key</param>
        /// <returns>复制结果，如果目标空间存在同名资源，不会覆盖。</returns>
        public HttpResult Copy(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey)
        {
#if DEBUG
            if (string.IsNullOrEmpty(sourceBucket))
            {
                throw new ArgumentNullException("sourceBucket");
            }
            if (string.IsNullOrEmpty(sourceKey))
            {
                throw new ArgumentNullException("sourceKey");
            }
            if (string.IsNullOrEmpty(destinationBucket))
            {
                throw new ArgumentNullException("destinationBucket");
            }
            if (string.IsNullOrEmpty(destinationKey))
            {
                throw new ArgumentNullException("destinationKey");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/copy/" + Base64.UrlSafeBase64Encode(sourceBucket, sourceKey) + "/" + Base64.UrlSafeBase64Encode(destinationBucket, destinationKey);
            string token = auth.CreateManageToken(url);
            return httpManager.Post(url, null, token);
        }

        /// <summary>
        /// 文件解压缩
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/decompression
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">需要解压缩的压缩文件</param>
        /// <param name="format">解压的文件格式，支持zip、tar、gzip、7z</param>
        /// <param name="directory">可选，解压文件到指定目录，参数中需要填入"空间:目录"</param>
        /// <param name="saveList">可选，将解压缩生成的list保存为指定文件，参数中需要填入"空间:文件名"</param>
        /// <returns>复制结果，如果目标空间存在同名资源，不会覆盖。</returns>
        public HttpResult Decompression(string bucket, string key, string format, string directory = null, string saveList = null, string notifyURL = null, int force = -1, int separate = -1)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentNullException("format");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/fops";
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("bucket", Base64.UrlSafeBase64Encode(bucket));
            query.Add("key", Base64.UrlSafeBase64Encode(key));

            string fops = "decompression/" + format;
            if (!string.IsNullOrEmpty(directory))
            {
                fops += "/dir/" + Base64.UrlSafeBase64Encode(directory);
            }
            if (!string.IsNullOrEmpty(saveList))
            {
                fops += "|saveas/" + Base64.UrlSafeBase64Encode(saveList);
            }
            query.Add("fops", Base64.UrlSafeBase64Encode(fops));

            if (!string.IsNullOrEmpty(notifyURL))
            {
                query.Add("notifyURL", Base64.UrlSafeBase64Encode(notifyURL));
            }
            if (0 == force || 1 == force)
            {
                query.Add("force", force.ToString());
            }
            if (0 == separate || 1 == separate)
            {
                query.Add("separate", separate.ToString());
            }
            byte[] body = UrlUtility.MakeQueryData(query);
            string token = auth.CreateManageToken(url, body);
            return httpManager.Post(url, body, token);
        }

        /// <summary>
        /// 文件解压缩
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/decompression
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">需要解压缩的压缩文件</param>
        /// <param name="format">解压的文件格式，支持zip、tar、gzip、7z</param>
        /// <param name="directory">可选，解压文件到指定目录，参数中需要填入"空间:目录"</param>
        /// <param name="saveList">可选，将解压缩生成的list保存为指定文件，参数中需要填入"空间:文件名"</param>
        /// <returns>复制结果，如果目标空间存在同名资源，不会覆盖。</returns>
        public HttpResult Decompression(string bucket, string key, string format, Dictionary<string, string> directoryAndSaveList, string notifyURL = null, int force = -1, int separate = -1)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentNullException("format");
            }
            if (null == directoryAndSaveList || 0 == directoryAndSaveList.Count)
            {
                throw new ArgumentNullException("directoryAndSaveList");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/fops";
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("bucket", Base64.UrlSafeBase64Encode(bucket));
            query.Add("key", Base64.UrlSafeBase64Encode(key));

            StringBuilder sb = new StringBuilder();
            foreach(var d in directoryAndSaveList)
            {
                sb.Append("decompression/" + format);
                sb.Append("/dir/" + Base64.UrlSafeBase64Encode(d.Key));
                if (!string.IsNullOrEmpty(d.Value))
                {
                    sb.Append("|saveas/" + Base64.UrlSafeBase64Encode(d.Value));
                }
                sb.Append(";");
            }
            query.Add("fops", Base64.UrlSafeBase64Encode(sb.ToString(0, sb.Length - 1)));

            if (!string.IsNullOrEmpty(notifyURL))
            {
                query.Add("notifyURL", Base64.UrlSafeBase64Encode(notifyURL));
            }
            if (0 == force || 1 == force)
            {
                query.Add("force", force.ToString());
            }
            if (0 == separate || 1 == separate)
            {
                query.Add("separate", separate.ToString());
            }
            byte[] body = UrlUtility.MakeQueryData(query);
            string token = auth.CreateManageToken(url, body);
            return httpManager.Post(url, body, token);
        }

        /// <summary>
        /// 设置文件保存期限
        /// https://wcs.chinanetcenter.com/document/API/ResourceManage/setdeadline
        /// 超过设置的天数文件自动删除。注：文件删除后不可恢复，请谨慎操作。
        /// </summary>
        /// <param name="bucket">空间名称</param>
        /// <param name="key">文件 key</param>
        /// <param name="deadline">文件保存期限。超过保存天数文件自动删除,单位：天。例如：1、2、3……注：0表示尽快删除，-1表示取消过期时间，永久保存</param>
        /// <param name="relevance">
        /// 可选，操作m3u8文件时是否关联设置TS文件的保存期限。 
        /// 0 不进行关联设置 
        /// 1 关联设置
        /// 注：若未填写该参数，默认为关联操作。若为非m3u8文件，该参数不生效
        /// </param>
        /// <returns>设置结果</returns>
        public HttpResult SetDeadline(string bucket, string key, int deadline, int relevance = -1)
        {
#if DEBUG
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
#endif
            string url = config.GetManageUrlPrefix() + "/setdeadline";
            string body = "bucket=" + Base64.UrlSafeBase64Encode(bucket)
                + "&key=" + Base64.UrlSafeBase64Encode(key)
                + "&deadline=" + deadline.ToString();
            if (0 == relevance || 1 == relevance)
            {
                body += "&relevance=" + relevance.ToString();
            }
            string token = auth.CreateManageToken(url, body);
            return httpManager.PostText(url, body, token);
        }

        #endregion Change

        private Auth auth;
        private HttpManager httpManager;
        private Config config;
    }
}
