﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace PRISM
{
    /// <summary>
    /// Methods for computing checksum hashes for files
    /// </summary>
    public class HashUtilities
    {
        /// <summary>
        /// Default date/time format
        /// </summary>
        public const string DATE_TIME_FORMAT = "yyyy-MM-dd hh:mm:ss tt";

        /// <summary>
        /// Hashcheck file suffix
        /// </summary>
        public const string HASHCHECK_FILE_SUFFIX = ".hashcheck";

        private const string UNDEFINED_HASH = "undefined";

        private const string CRC32_HASH = "crc32";

        private const string MD5_HASH = "md5";

        private const string SHA1_HASH = "sha1";

        /// <summary>
        /// Hash type constants
        /// </summary>
        public enum HashTypeConstants
        {
            /// <summary>
            /// Undefined
            /// </summary>
            Undefined = 0,

            /// <summary>
            /// CRC32
            /// </summary>
            CRC32 = 1,

            /// <summary>
            /// MD5
            /// </summary>
            MD5 = 2,

            /// <summary>
            /// SHA1
            /// </summary>
            SHA1 = 3
        }

        /// <summary>
        /// Hash info, tracking file size, date, hash, and hash type
        /// </summary>
        public struct HashInfoType
        {
            /// <summary>
            /// File size, in bytes
            /// </summary>
            public long FileSize;

            /// <summary>
            /// File modification date (UTC)
            /// </summary>
            public DateTime FileDateUtc;

            /// <summary>
            /// Hash type (typically MD5 or SHA1)
            /// </summary>
            public HashTypeConstants HashType;

            /// <summary>
            /// Have value
            /// </summary>
            public string HashValue;

            /// <summary>
            /// Reset values to defaults
            /// </summary>
            /// <remarks>HashType will be Undefined</remarks>
            public void Clear()
            {
                FileSize = 0;
                FileDateUtc = DateTime.MinValue;
                HashType = HashTypeConstants.Undefined;
                HashValue = string.Empty;
            }
        }

        /// <summary>
        /// Converts a byte array into a hex string
        /// </summary>
        private static string ByteArrayToString(byte[] arrInput)
        {

            var output = new StringBuilder(arrInput.Length);

            for (var i = 0; i <= arrInput.Length - 1; i++)
            {
                output.Append(arrInput[i].ToString("X2"));
            }

            return output.ToString().ToLower();

        }

        /// <summary>
        /// Computes the CRC32 hash for a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ComputeFileHashCrc32(string filePath)
        {
            string hashValue;

            // Open file (as read-only)
            using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Hash contents of this stream
                hashValue = ComputeCRC32Hash(reader);
            }

            return hashValue;
        }

        /// <summary>
        /// Computes the MD5 hash for a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ComputeFileHashMD5(string filePath)
        {

            string hashValue;

            // Open file (as read-only)
            using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Hash contents of this stream
                hashValue = ComputeMD5Hash(reader);
            }

            return hashValue;

        }

        /// <summary>
        /// Computes the MD5 hash for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        // ReSharper disable once UnusedMember.Global
        public static string ComputeStringHashMD5(string text)
        {

            var hashValue = ComputeMD5Hash(new MemoryStream(Encoding.UTF8.GetBytes(text)));

            return hashValue;

        }

        /// <summary>
        /// Computes the hash for a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hashType">Hash type</param>
        /// <returns>Hash value</returns>
        public static string ComputeFileHash(string filePath, HashTypeConstants hashType)
        {
            if (hashType == HashTypeConstants.Undefined)
                hashType = HashTypeConstants.SHA1;

            switch (hashType)
            {
                case HashTypeConstants.CRC32:
                    return ComputeFileHashCrc32(filePath);
                case HashTypeConstants.MD5:
                    return ComputeFileHashMD5(filePath);
                case HashTypeConstants.SHA1:
                    return ComputeFileHashSha1(filePath);
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashType), "Unknown hash type");
            }

        }

        /// <summary>
        /// Computes the SHA-1 hash for a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ComputeFileHashSha1(string filePath)
        {

            string hashValue;

            // open file (as read-only)
            using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Hash contents of this stream
                hashValue = ComputeSha1Hash(reader);
            }

            return hashValue;

        }

        /// <summary>
        /// Computes the SHA-1 hash for a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        // ReSharper disable once UnusedMember.Global
        public static string ComputeStringHashSha1(string text)
        {

            var hashValue = ComputeSha1Hash(new MemoryStream(Encoding.UTF8.GetBytes(text)));

            return hashValue;

        }

        /// <summary>
        /// Computes the CRC32 hash of a given stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns>CRC32 hash, as a string</returns>
        /// <remarks></remarks>
        private static string ComputeCRC32Hash(Stream data)
        {
            var crc = Crc32.Crc(data);
            var crcString = string.Format("{0:X8}", crc);
            return crcString;
        }

        /// <summary>
        /// Computes the MD5 hash of a given stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns>MD5 hash, as a string</returns>
        /// <remarks></remarks>
        private static string ComputeMD5Hash(Stream data)
        {

            var md5Hasher = new MD5CryptoServiceProvider();
            return ComputeHash(md5Hasher, data);

        }

        /// <summary>
        /// Computes the SHA-1 hash of a given stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns>SHA1 hash, as a string</returns>
        /// <remarks></remarks>
        private static string ComputeSha1Hash(Stream data)
        {

            var sha1Hasher = new SHA1CryptoServiceProvider();
            return ComputeHash(sha1Hasher, data);

        }

        /// <summary>
        /// Use the given hash algorithm to compute a hash of the data stream
        /// </summary>
        /// <param name="hasher"></param>
        /// <param name="data"></param>
        /// <returns>Hash string</returns>
        /// <remarks></remarks>
        private static string ComputeHash(HashAlgorithm hasher, Stream data)
        {
            // hash contents of this stream
            var arrHash = hasher.ComputeHash(data);

            // Return the hash, formatted as a string
            return ByteArrayToString(arrHash);

        }

        /// <summary>
        /// Creates a .hashcheck file for the specified file
        /// The file will be created in the same directory as the data file, and will contain size, modification_date_utc, and hash
        /// </summary>
        /// <param name="dataFilePath">File path to hash</param>
        /// <param name="hashType">Hash type</param>
        /// <param name="hashValue">Output: the computed file hash</param>
        /// <param name="warningMessage">Output: warning message</param>
        /// <returns>The full path to the .hashcheck file; empty string if a problem</returns>
        /// <remarks></remarks>
        public static string CreateHashcheckFile(
            string dataFilePath,
            HashTypeConstants hashType,
            out string hashValue,
            out string warningMessage)
        {

            if (!File.Exists(dataFilePath))
            {
                hashValue = string.Empty;
                warningMessage = "Cannot compute .hashcheck file; source file not found: " + dataFilePath;
                return string.Empty;
            }

            hashValue = ComputeFileHash(dataFilePath, hashType);

            try
            {
                var hashcheckFilePath = CreateHashcheckFileWithHash(dataFilePath, hashType, hashValue, out warningMessage);
                return hashcheckFilePath;
            }
            catch (Exception ex)
            {
                // Treat this as a non-critical error
                warningMessage = string.Format("Unable to create the .hashcheck file for source file {0}: {1}",
                                               dataFilePath, ex.Message);
                return string.Empty;
            }

        }

        /// <summary>
        /// Creates a .hashcheck file for the specified file
        /// The file will be created in the same directory as the data file, and will contain size, modification_date_utc, hash, and hashtype
        /// </summary>
        /// <param name="dataFilePath">File path to hash</param>
        /// <param name="hashType">Hash type</param>
        /// <param name="hashValue">Output: the computed file hash</param>
        /// <param name="warningMessage">Output: warning message</param>
        /// <returns>The full path to the .hashcheck file; empty string if a problem</returns>
        /// <remarks></remarks>
        public static string CreateHashcheckFileWithHash(
            string dataFilePath,
            HashTypeConstants hashType,
            string hashValue,
            out string warningMessage)
        {

            var fiDataFile = new FileInfo(dataFilePath);

            if (!fiDataFile.Exists)
            {
                warningMessage = "Cannot create .hashcheck file; source file not found: " + fiDataFile.FullName;
                return string.Empty;
            }

            var hashCheckFilePath = fiDataFile.FullName + HASHCHECK_FILE_SUFFIX;
            if (string.IsNullOrWhiteSpace(hashValue))
                hashValue = string.Empty;

            string hashTypeDescription;
            switch (hashType)
            {
                case HashTypeConstants.Undefined:
                    hashTypeDescription = UNDEFINED_HASH;
                    break;
                case HashTypeConstants.CRC32:
                    hashTypeDescription = CRC32_HASH;
                    break;
                case HashTypeConstants.MD5:
                    hashTypeDescription = MD5_HASH;
                    break;
                case HashTypeConstants.SHA1:
                    hashTypeDescription = SHA1_HASH;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashType), "Unknown hash type");
            }

            try
            {

                using (var swOutFile = new StreamWriter(new FileStream(hashCheckFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    swOutFile.WriteLine("# Hashcheck file created " + DateTime.Now.ToString(DATE_TIME_FORMAT));
                    swOutFile.WriteLine("size=" + fiDataFile.Length);
                    swOutFile.WriteLine("modification_date_utc=" + fiDataFile.LastWriteTimeUtc.ToString(DATE_TIME_FORMAT));
                    swOutFile.WriteLine("hash=" + hashValue);
                    swOutFile.WriteLine("hashtype=" + hashTypeDescription);
                }

                warningMessage = string.Empty;
                return hashCheckFilePath;
            }
            catch (Exception ex)
            {
                // Treat this as a non-critical error
                warningMessage = string.Format("Unable to create .hashcheck file {0}: {1}", hashCheckFilePath, ex.Message);
                return string.Empty;
            }

        }

        /// <summary>
        /// Read the data in an existing hashcheck file
        /// </summary>
        /// <param name="hashCheckFilePath"></param>
        /// <param name="assumedHashType">
        /// Hashtype to assume if the .hashcheck file does not have "hashtype" defined and if the hash length is not 8, 32, or 40
        /// </param>
        /// <returns>Hash info</returns>
        public static HashInfoType ReadHashcheckFile(string hashCheckFilePath, HashTypeConstants assumedHashType = HashTypeConstants.Undefined)
        {
            var splitChar = new[] {'='};

            var hashInfo = new HashInfoType();
            hashInfo.Clear();

            using (var reader = new StreamReader(new FileStream(hashCheckFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(dataLine) || dataLine.Trim().StartsWith("#") || !dataLine.Contains("="))
                    {
                        continue;
                    }

                    var dataColumns = dataLine.Trim().Split(splitChar, 2);
                    if (dataColumns.Length > 2)
                        continue;

                    switch (dataColumns[0].ToLower())
                    {
                        case "size":
                            long.TryParse(dataColumns[1], out hashInfo.FileSize);
                            break;
                        case "date":
                        case "modification_date_utc":
                            DateTime.TryParse(dataColumns[1], out hashInfo.FileDateUtc);
                            break;
                        case "hash":
                            hashInfo.HashValue = dataColumns[1];
                            break;
                        case "hashtype":

                            switch (dataColumns[1].ToLower())
                            {
                                case CRC32_HASH:
                                    hashInfo.HashType = HashTypeConstants.CRC32;
                                    break;
                                case MD5_HASH:
                                    hashInfo.HashType = HashTypeConstants.MD5;
                                    break;
                                case SHA1_HASH:
                                    hashInfo.HashType = HashTypeConstants.SHA1;
                                    break;
                                default:
                                    hashInfo.HashType = HashTypeConstants.Undefined;
                                    break;
                            }
                            break;
                    }
                }
            }

            if (hashInfo.HashType != HashTypeConstants.Undefined)
                return hashInfo;

            if (hashInfo.HashValue.Length == 8)
                hashInfo.HashType = HashTypeConstants.CRC32;
            else if (hashInfo.HashValue.Length == 32)
                hashInfo.HashType = HashTypeConstants.MD5;
            else if (hashInfo.HashValue.Length == 40)
                hashInfo.HashType = HashTypeConstants.SHA1;
            else
                hashInfo.HashType = assumedHashType;

            return hashInfo;
        }
    }
}
