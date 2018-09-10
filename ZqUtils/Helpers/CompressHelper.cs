using System;
using System.IO;
using System.IO.Compression;
/****************************
* [Author] 张强
* [Date] 2016-01-06
* [Describe] 解压缩工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 解压缩工具类
    /// </summary>
    public class CompressHelper
    {
        #region gzip
        /// <summary>
        /// gzip压缩字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] GZipCompress(byte[] arr)
        {
            byte[] res = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        gZipStream.Write(arr, 0, arr.Length);
                    }
                    res = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "gzip压缩字节数组");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// gzip解压字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>解压缩后的字节数组</returns>
        public static byte[] GZipDecompress(byte[] arr)
        {
            byte[] res = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var stream = new MemoryStream(arr))
                    {
                        using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            gZipStream.CopyTo(ms, 10240);
                            res = ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "gzip解压字节数组");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// gzip压缩字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>压缩后的字节流</returns>
        public static Stream GZipCompress(Stream sourceStream)
        {
            Stream res = null;
            try
            {
                using (sourceStream)
                {
                    var bytesArr = new byte[sourceStream.Length];
                    sourceStream.Read(bytesArr, 0, bytesArr.Length);
                    var gZipArr = GZipCompress(bytesArr);
                    res = new MemoryStream(gZipArr);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "gzip压缩字节流");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// gzip解压字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>解压缩后的字节流</returns>
        public static Stream GZipDecompress(Stream sourceStream)
        {
            Stream res = null;
            try
            {
                using (sourceStream)
                {
                    var bytesArr = new byte[sourceStream.Length];
                    sourceStream.Read(bytesArr, 0, bytesArr.Length);
                    var gZipArr = GZipDecompress(bytesArr);
                    res = new MemoryStream(gZipArr);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "gzip解压字节流");
                res = null;
            }
            return res;
        }
        #endregion

        #region deflate
        /// <summary>
        /// deflate压缩字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] DeflateCompress(byte[] arr)
        {
            byte[] res = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var gZipStream = new DeflateStream(stream, CompressionMode.Compress))
                    {
                        gZipStream.Write(arr, 0, arr.Length);
                    }
                    res = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "deflate压缩字节数组");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// deflate解压字节数组
        /// </summary>
        /// <param name="arr">源字节数组</param>
        /// <returns>解压缩后的字节数组</returns>
        public static byte[] DeflateDecompress(byte[] arr)
        {
            byte[] res = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var stream = new MemoryStream(arr))
                    {
                        using (var gZipStream = new DeflateStream(stream, CompressionMode.Decompress))
                        {
                            gZipStream.CopyTo(ms, 10240);
                            res = ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "deflate解压字节数组");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// deflate压缩字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>压缩后的字节流</returns>
        public static Stream DeflateCompress(Stream sourceStream)
        {
            Stream res = null;
            try
            {
                using (sourceStream)
                {
                    var bytesArr = new byte[sourceStream.Length];
                    sourceStream.Read(bytesArr, 0, bytesArr.Length);
                    var gZipArr = DeflateCompress(bytesArr);
                    res = new MemoryStream(gZipArr);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "deflate压缩字节流");
                res = null;
            }
            return res;
        }

        /// <summary>
        /// deflate解压字节流
        /// </summary>
        /// <param name="sourceStream">源字节流</param>
        /// <returns>解压缩后的字节流</returns>
        public static Stream DeflateDecompress(Stream sourceStream)
        {
            Stream res = null;
            try
            {
                using (sourceStream)
                {
                    var bytesArr = new byte[sourceStream.Length];
                    sourceStream.Read(bytesArr, 0, bytesArr.Length);
                    var gZipArr = DeflateDecompress(bytesArr);
                    res = new MemoryStream(gZipArr);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "deflate解压字节流");
                res = null;
            }
            return res;
        }
        #endregion
    }
}
