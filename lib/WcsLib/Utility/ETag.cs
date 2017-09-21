using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wangsu.WcsLib.Utility
{
    /// <summary>
    /// HTTP 协议：ETag == URL 的 Entity Tag，用于标示 URL 对象是否改变，区分不同语言和 Session 等等。
    /// </summary>
    public sealed class ETag
    {
        public static string ComputeEtag(string filePath)
        {
            string etag = "";

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = stream.Length;
                    byte[] block = new byte[BLOCK_SIZE];
                    byte[] tag = new byte[SHA1_VALUE_SIZE + 1];
                    if (fileLength <= BLOCK_SIZE)
                    {
                        int readBytes = stream.Read(block, 0, BLOCK_SIZE);
                        byte[] sha1 = Hash.ComputeSha1(block, 0, readBytes);

                        tag[0] = 0x16;
                        //Array.Copy(sha1, 0, tag, 1, sha1.Length);
                        Buffer.BlockCopy(sha1, 0, tag, 1, sha1.Length);
                    }
                    else
                    {
                        long blockCount = (fileLength + BLOCK_SIZE - 1) / BLOCK_SIZE;
                        byte[] allBlocksSha1 = new byte[SHA1_VALUE_SIZE * blockCount];

                        for (int i = 0; i < blockCount; i++)
                        {
                            int readBytes = stream.Read(block, 0, BLOCK_SIZE);
                            byte[] currentBlockSha1 = Hash.ComputeSha1(block, 0, readBytes);
                            Buffer.BlockCopy(currentBlockSha1, 0, allBlocksSha1, i * SHA1_VALUE_SIZE, currentBlockSha1.Length);
                        }

                        byte[] sha1 = Hash.ComputeSha1(allBlocksSha1);

                        tag[0] = 0x96;
                        Buffer.BlockCopy(sha1, 0, tag, 1, sha1.Length);
                    }
                    etag = Base64.UrlSafeBase64Encode(tag);
                }
            }
            catch (Exception)
            {
            }

            return etag;
        }

        public static string ComputeEtag(byte[] data)
        {
            byte[] tag = new byte[SHA1_VALUE_SIZE + 1];
            if (data.Length <= BLOCK_SIZE)
            {
                byte[] sha1 = Hash.ComputeSha1(data, 0, data.Length);

                tag[0] = 0x16;
                Buffer.BlockCopy(sha1, 0, tag, 1, sha1.Length);
            }
            else
            {
                long blockCount = (data.Length + BLOCK_SIZE - 1) / BLOCK_SIZE;
                byte[] allBlocksSha1 = new byte[SHA1_VALUE_SIZE * blockCount];

                for (int i = 0; i < blockCount; i++)
                {
                    int readBytes = i < blockCount - 1 ? BLOCK_SIZE : data.Length - BLOCK_SIZE * i;
                    byte[] currentBlockSha1 = Hash.ComputeSha1(data, BLOCK_SIZE * i, readBytes);
                    Buffer.BlockCopy(currentBlockSha1, 0, allBlocksSha1, i * SHA1_VALUE_SIZE, currentBlockSha1.Length);
                }

                byte[] sha1 = Hash.ComputeSha1(allBlocksSha1);

                tag[0] = 0x96;
                Buffer.BlockCopy(sha1, 0, tag, 1, sha1.Length);
            }
            return Base64.UrlSafeBase64Encode(tag);
        }

        // 块大小
        private const int BLOCK_SIZE = 4 * 1024 * 1024;

        private const int SHA1_VALUE_SIZE = 20;
    }
}
