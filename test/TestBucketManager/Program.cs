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

// 设置 Auth 的 3 种方法：
// 1、命令行下使用 set WcsLibAkSk=<Ak>,<Sk>
//    这种方式只设置当前命令行会话，关闭后设置就消失，每次新开都要设置。

// 2、命令行下使用 setx WcsLibAkSk <Ak>,<Sk>
//    【推荐】这种方式保存设置到用户环境变量中

// 3、命令行下使用 setx /m WcsLibAkSk <Ak>,<Sk>
//    这种方式保存设置到系统环境变量中

namespace TestBucketManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Auth auth = EnvUtility.EnvAuth();
            Config config = EnvUtility.EnvConfig();
            
            BucketManager bm = new BucketManager(auth, config);

            // 枚举所有 Buckets
            HttpResult result = bm.Buckets();
            Console.WriteLine("---Buckets---\n{0}", result.ToString());
            if ((int)HttpStatusCode.OK == result.Code)
            {
                JObject jo = JObject.Parse(result.Text);
                var buckets = jo["buckets"];
                Console.WriteLine("{0} Buckets found:", buckets.Count());
                int i = 0;
                foreach (var bucket in buckets)
                {
                    // 打印 Bucket 名字
                    Console.WriteLine("{0}\t{1}", ++i, bucket["name"].ToString());
                }
            }

            // BucketManager 还有很多函数，以下示范比较粗糙，您需要自己改改！
            result = bm.BucketList("umu618-docs", 0, null, 1);
            Console.WriteLine("---BucketList---\n{0}", result.ToString());

            result = bm.BucketStat("chentemp|umu618-docs", "2017-07-01", "2017-09-01");
            Console.WriteLine("---BucketStat---\n{0}", result.ToString());

            result = bm.Stat("umu618-docs", "Go.pdf");
            Console.WriteLine("---Stat---\n{0}", result.ToString());

            string[] keys = { "Go.pdf" };
            result = bm.Prefetch("umu618-docs", keys);
            Console.WriteLine("---Prefetch---\n{0}", result.ToString());

            result = bm.ImageInfo("http://images.w.wcsapi.biz.matocloud.com/1.png");
            Console.WriteLine("---ImageInfo---\n{0}", result.ToString());

            result = bm.Exif("http://chentemp.w.wcsapi.biz.matocloud.com/IMG_1140.JPG");
            Console.WriteLine("---Exif---\n{0}", result.ToString());

            result = bm.AvInfo("http://images.w.wcsapi.biz.matocloud.com/1.mp4");
            Console.WriteLine("---AvInfo---\n{0}", result.ToString());

            result = bm.AvInfo2("http://images.w.wcsapi.biz.matocloud.com/1.mp4");
            Console.WriteLine("---AvInfo2---\n{0}", result.ToString());

            result = bm.Delete("umu618-docs", "/temp/");
            Console.WriteLine("---Delete---\n{0}", result.ToString());

            result = bm.Move("umu618-docs", "Go.pdf", "umu618-docs", "golang/Go.pdf");
            Console.WriteLine("---Move---\n{0}", result.ToString());

            result = bm.Copy("umu618-docs", "data.txt", "umu618-docs", "data.txt");
            Console.WriteLine("---Copy---\n{0}", result.ToString());

            result = bm.Decompression("umu618-docs", "各种录音程序.7z", "7z", "umu618-docs:exe", "umu618-docs:exe/UMU.list");
            Console.WriteLine("---Decompression---\n{0}", result.ToString());

            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("umu618-docs:/", "umu618-docs:UMU.list");
            d.Add("umu618-docs:temp", "umu618-docs:temp/UMU.list");
            result = bm.Decompression("umu618-docs", "各种录音程序.7z", "7z", d);
            Console.WriteLine("---Decompression---\n{0}", result.ToString());

            result = bm.PersistentStatus("e534f75d41534f468a7efe9a51dc90ad");
            Console.WriteLine("---PersistentStatus---\n{0}", result.ToString());

            result = bm.SetDeadline("umu618-docs", "Go.pdf", -1);
            Console.WriteLine("---SetDeadline---\n{0}", result.ToString());
        }
    }
}
