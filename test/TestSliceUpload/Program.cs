using Newtonsoft.Json.Linq;
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

namespace TestSliceUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            Auth auth = EnvUtility.EnvAuth();
            Config config = EnvUtility.EnvConfig();

            // 上传到这个 bucket
            string bucket = "umu618-docs";
            string key = "U-6M.txt";

            // 上传前先删除
            BucketManager bm = new BucketManager(auth, config);
            HttpResult result = bm.Delete(bucket, key);
            Console.WriteLine("---Delete---\n{0}", result.ToString());

            // 在内存构造一个文件内容：2M 个 U，2M 个 M， 2M 个 U
            const long dataSize = 6 * 1024 * 1024;
            byte[] data = new byte[dataSize];
            int i = 0;
            for (; i < dataSize / 3; ++i)
            {
                data[i] = 85;
            }
            for (; i < dataSize / 3 * 2; ++i)
            {
                data[i] = 77;
            }
            for (; i < dataSize; ++i)
            {
                data[i] = 85;
            }

            // 最后合成文件时的 hash
            Console.WriteLine("ETag of uploading data: {0}", ETag.ComputeEtag(data));

            // 一个小时的超时，转为 UnixTime 毫秒数
            long deadline = UnixTimestamp.GetUnixTimestamp(3600) * 1000;
            string putPolicy = "{\"scope\": \"" + bucket + ":" + key + "\",\"deadline\": \"" + deadline + "\"}";
            Console.WriteLine("----putPolicy----\n{0}", putPolicy);
            string uploadToken = auth.CreateUploadToken(putPolicy);
            Console.WriteLine("----uploadToken----\n{0}", uploadToken);

            // 第一个分片不宜太大，因为可能遇到错误，上传太大是白费流量和时间！
            const long blockSize = 4 * 1024 * 1024;
            const int firstChunkSize = 1024;

            SliceUpload su = new SliceUpload(auth, config);
            result = su.MakeBlock(blockSize, 0, data, 0, firstChunkSize, uploadToken);
            Console.WriteLine("---MakeBlock---\n{0}", result.ToString());

            if ((int)HttpStatusCode.OK == result.Code)
            {
                long blockCount = (dataSize + blockSize - 1) / blockSize;
                string[] contexts = new string[blockCount];

                JObject jo = JObject.Parse(result.Text);
                
                contexts[0] = jo["ctx"].ToString();

                // 上传第 1 个 block 剩下的数据
                result = su.Bput(contexts[0], firstChunkSize, data, firstChunkSize, (int)(blockSize - firstChunkSize), uploadToken);
                Console.WriteLine("---Bput---\n{0}", result.ToString());
                if ((int)HttpStatusCode.OK == result.Code)
                {
                    jo = JObject.Parse(result.Text);
                    contexts[0] = jo["ctx"].ToString();

                    // 上传后续 block，每次都是一整块上传
                    for (int blockIndex = 1; blockIndex < blockCount; ++blockIndex)
                    {
                        long leftSize = dataSize - blockSize * blockIndex;
                        int chunkSize = (int)(leftSize > blockSize ? blockSize : leftSize);
                        result = su.MakeBlock(chunkSize, blockIndex, data, (int)(blockSize * blockIndex), chunkSize, uploadToken);
                        Console.WriteLine("---MakeBlock---\n{0}", result.ToString());
                        if ((int)HttpStatusCode.OK == result.Code)
                        {
                            jo = JObject.Parse(result.Text);
                            contexts[blockIndex] = jo["ctx"].ToString();
                        }
                        else
                        {
                            Console.WriteLine("----Exit with error----");
                            return;
                        }
                    }

                    // 合成文件，注意与前面打印的 ETag 对比
                    result = su.MakeFile(dataSize, key, contexts, uploadToken);
                    Console.WriteLine("---MakeFile---\n{0}", result.ToString());
                }
            }
        }
    }
}
