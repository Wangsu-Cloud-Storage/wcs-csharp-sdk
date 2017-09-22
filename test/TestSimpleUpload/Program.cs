using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestCommon;
using Wangsu.WcsLib.Core;
using Wangsu.WcsLib.HTTP;
using Wangsu.WcsLib.Utility;

namespace TestSimpleUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            Auth auth = EnvUtility.EnvAuth();
            Config config = EnvUtility.EnvConfig();

            // 上传到这个 bucket
            string bucket = "umu618-docs";
            string key = "U-1K.txt";

            // 上传前先删除
            BucketManager bm = new BucketManager(auth, config);
            HttpResult result = bm.Delete(bucket, key);
            Console.WriteLine("---Delete---\n{0}", result.ToString());

            // 在内存构造一个文件内容：1024 个 U
            const long dataSize = 1024;
            byte[] data = new byte[dataSize];
            for (int i = 0; i < dataSize; ++i)
            {
                data[i] = 85;
            }

            // 一个小时的超时，转为 UnixTime 毫秒数
            long deadline = UnixTimestamp.GetUnixTimestamp(3600) * 1000;
            string putPolicy = "{\"scope\": \"" + bucket + "\",\"deadline\": \"" + deadline + "\"}";
            Console.WriteLine("----putPolicy----\n{0}", putPolicy);

            SimpleUpload su = new SimpleUpload(auth, config);
            result = su.UploadData(data, putPolicy, key);
            Console.WriteLine("---UploadData---\n{0}", result.ToString());

            // 下面输出的 HASH 值应该一样
            if ((int)HttpStatusCode.OK == result.Code)
            {
                Console.WriteLine("UploadResult: {0}", Base64.UrlSafeBase64Decode(result.Text));
            }

            Console.WriteLine("UploadData's ETag: {0}", ETag.ComputeEtag(data));
        }
    }
}
