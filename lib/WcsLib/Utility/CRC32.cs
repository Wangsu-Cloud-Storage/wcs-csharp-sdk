using System;
using System.Collections.Generic;
using System.Linq;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif
using System.Text;
using System.Threading.Tasks;

namespace Wangsu.WcsLib.Utility
{
    /// <summary>
    /// CRC32, LE, Polynomial == 0xEDB88320, XorOut == 0xFFFFFFFF
    /// </summary>
    public sealed class CRC32
    {
        /// <summary>
        /// magic
        /// </summary>
        public const uint IEEE8023 = 0xEDB88320;

        /// <summary>
        /// 初始化
        /// </summary>
        public CRC32(uint polynomial = IEEE8023, uint xorOut = 0xFFFFFFFF)
        {
            XorOut = xorOut;
            Value = 0;
            Table = MakeTable(polynomial);
        }

        /// <summary>
        /// 校验和
        /// </summary>
        /// <returns>校验和</returns>
        public uint GetCheckSum()
        {
            return Value;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="data">字节数据</param>
        /// <param name="offset">偏移位置</param>
        /// <param name="count">字节数</param>
        /// <returns>CheckSum</returns>
        public uint Update(byte[] data, int offset, int count)
        {
            uint crc = Value ^ XorOut;
            for (int i = 0; i < count; i++)
            {
                crc = Table[((byte)crc) ^ data[offset + i]] ^ (crc >> 8);
            }
            Value = crc ^ XorOut;
            return Value;
        }

        public void Reset()
        {
            Value = 0;
        }

        #region PRIVATE
        private static uint[] MakeTable(uint polynomial)
        {
            uint[] table = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                uint crc = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                table[i] = crc;
            }
            return table;
        }

        private uint XorOut;
        private uint[] Table;
        private uint Value;
        #endregion
    }
}
