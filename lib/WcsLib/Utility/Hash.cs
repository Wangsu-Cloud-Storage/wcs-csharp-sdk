using System;
using System.Text;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif

namespace Wangsu.WcsLib.Utility
{
    /// <summary>
    /// 计算 Hash 值
    /// </summary>
    public sealed class Hash
    {
        /// <summary>
        /// 计算 SHA1
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <param name="offset">数组偏移量</param>
        /// <param name="count">数组长度</param>
        /// <returns>SHA1 20 Bytes</returns>
        public static byte[] ComputeSha1(byte[] data, int offset, int count)
        {
#if WINDOWS_UWP
            var sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var buffer = CryptographicBuffer.CreateFromByteArray(ArraySlice(data, offset, count));
            var digest = sha1.HashData(buffer);
            var hashBytes = new byte[count];
            CryptographicBuffer.CopyToByteArray(digest, out hashBytes);
            return hashBytes;
#else
            SHA1 sha1 = SHA1.Create();
            return sha1.ComputeHash(data, offset, count);
#endif
        }

        /// <summary>
        /// 计算 SHA1
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <returns>SHA1 20 Bytes</returns>
        public static byte[] ComputeSha1(byte[] data)
        {
            return ComputeSha1(data, 0, data.Length);
        }

        /// <summary>
        /// 计算字符串 SHA1
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>SHA1 20 Bytes</returns>
        public static byte[] ComputeSha1(string str)
        {
            return ComputeSha1(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 计算字节数组 MD5 哈希(开启 FIPS 时会抛异常，这是您为了安全而开启的，所以就应该崩溃，让安全问题充分暴露；反而是那些不崩溃的，那是在蒙蔽您！)
        /// </summary>
        /// <param name="data">待计算的字节数组</param>
        /// <param name="offset">数组偏移量</param>
        /// <param name="count">数组长度</param>
        /// <returns>MD5 结果</returns>
        public static byte[] ComputeMd5(byte[] data, int offset, int count)
        {
#if WINDOWS_UWP
            var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.CreateFromByteArray(ArraySlice(data, offset, count));
            var digest = md5.HashData(buffer);
            var hashBytes = new byte[digest.Length];
            CryptographicBuffer.CopyToByteArray(digest, out hashBytes);
            return hashBytes;
#else
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(data);
#endif
        }

        /// <summary>
        /// 计算字节数组 MD5 哈希(开启 FIPS 时会抛异常，这是您为了安全而开启的，所以就应该崩溃，让安全问题充分暴露；反而是那些不崩溃的，那是在蒙蔽您！)
        /// </summary>
        /// <param name="data">待计算的字节数组</param>
        /// <returns>MD5 结果</returns>
        public static byte[] ComputeMd5(byte[] data)
        {
            return ComputeMd5(data, 0, data.Length);
        }

        /// <summary>
        /// 计算字符串 MD5 哈希(开启 FIPS 时会抛异常，这是您为了安全而开启的，所以就应该崩溃，让安全问题充分暴露；反而是那些不崩溃的，那是在蒙蔽您！)
        /// </summary>
        /// <param name="str">待计算的字符串</param>
        /// <returns>MD5 结果</returns>
        public static byte[] ComputeMd5(string str)
        {
            return ComputeMd5(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 转为 Hex 字符串
        /// </summary>
        /// <param name="data">Hash 字节</param>
        /// <returns>Hex 字符串形式</returns>
        public static string ToHexString(byte[] data)
        {
#if WINDOWS_UWP
            return CryptographicBuffer.EncodeToHexString(CryptographicBuffer.CreateFromByteArray(data));
#else
            StringBuilder sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
#endif
        }

#if WINDOWS_UWP
        private static byte[] ArraySlice(byte[] data, int offset, int count)
        {
            if (offset + count > data.Length)
            {
                throw new System.IndexOutOfRangeException();
            }
            if (0 < offset || count < data.Length)
            {
                byte[] newArray = new byte[count];
                //Array.Copy(data, offset, newArray, 0, count);
                Buffer.BlockCopy(data, offset, newArray, 0, count);
                return newArray;
            }
            return data;
        }
#endif
    }
}
