using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CRSim.Core.Utils
{
    public class FileHashHelper
    {
        /// <summary>
        /// 计算文件 SHA256 值
        /// </summary>
        public static string ComputeSHA256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexStringLower(hashBytes);
        }

        /// <summary>
        /// 验证文件 SHA256
        /// </summary>
        public static bool VerifySHA256(string filePath, string expectedHash)
        {
            var fileHash = ComputeSHA256(filePath);
            return string.Equals(fileHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
