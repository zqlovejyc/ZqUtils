#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using ZqUtils.Reflection;
/****************************
* [Author] 张强
* [Date] 2018-05-21
* [Describe] Stream扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Stream扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region 压缩/解压缩 数据
        /// <summary>
        /// 压缩数据流
        /// </summary>
        /// <param name="this">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream Compress(this Stream @this, Stream outStream = null)
        {
            if (outStream == null) outStream = new MemoryStream();
            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
#if NET4
            using (var stream = new DeflateStream(outStream, CompressionMode.Compress, true))
#else
            using (var stream = new DeflateStream(outStream, CompressionLevel.Optimal, true))
#endif
            {
                @this.CopyTo(stream);
                stream.Flush();
                //stream.Close();
            }
            return outStream;
        }

        /// <summary>
        /// 解压缩数据流
        /// </summary>
        /// <param name="this">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream Decompress(this Stream @this, Stream outStream = null)
        {
            if (outStream == null) outStream = new MemoryStream();
            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new DeflateStream(@this, CompressionMode.Decompress, true))
            {
                stream.CopyTo(outStream);
                //stream.Close();
            }
            return outStream;
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <returns></returns>
        public static byte[] Compress(this byte[] @this)
        {
            var ms = new MemoryStream();
            Compress(new MemoryStream(@this), ms);
            return ms.ToArray();
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <returns></returns>
        public static byte[] Decompress(this byte[] @this)
        {
            var ms = new MemoryStream();
            Decompress(new MemoryStream(@this), ms);
            return ms.ToArray();
        }

        /// <summary>
        /// 压缩数据流
        /// </summary>
        /// <param name="this">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream CompressGZip(this Stream @this, Stream outStream = null)
        {
            if (outStream == null) outStream = new MemoryStream();
            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
#if NET4
            using (var stream = new GZipStream(outStream, CompressionMode.Compress, true))
#else
            using (var stream = new GZipStream(outStream, CompressionLevel.Optimal, true))
#endif
            {
                @this.CopyTo(stream);
                stream.Flush();
                //stream.Close();
            }
            return outStream;
        }

        /// <summary>
        /// 解压缩数据流
        /// </summary>
        /// <param name="this">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream DecompressGZip(this Stream @this, Stream outStream = null)
        {
            if (outStream == null) outStream = new MemoryStream();
            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new GZipStream(@this, CompressionMode.Decompress, true))
            {
                stream.CopyTo(outStream);
                //stream.Close();
            }
            return outStream;
        }
        #endregion

        #region 复制数据流
        /// <summary>
        /// 复制数据流
        /// </summary>
        /// <param name="this">源数据流</param>
        /// <param name="des">目的数据流</param>
        /// <param name="bufferSize">缓冲区大小，也就是每次复制的大小</param>
        /// <param name="max">最大复制字节数</param>
        /// <returns>返回复制的总字节数</returns>
        public static int CopyTo(this Stream @this, Stream des, int bufferSize = 0, int max = 0)
        {
            // 优化处理内存流，直接拿源内存流缓冲区往目标数据流里写
            if (@this is MemoryStream)
            {
                var ms = @this as MemoryStream;
                // 如果指针位于开头，并且要读完整个缓冲区，则直接使用WriteTo
                var count = (int)(ms.Length - ms.Position);
                if (ms.Position == 0 && (max <= 0 || count <= max))
                {
                    ms.WriteTo(des);
                    ms.Position = ms.Length;
                    return count;
                }
                // 反射读取内存流中数据的原始位置，然后直接把数据拿出来用
                if (ms.TryGetValue("_origin", out var obj))
                {
                    var _origin = (int)obj;
                    // 其实地址不为0时，一般不能直接访问缓冲区，因为可能被限制访问
                    var buf = ms.GetValue("_buffer") as byte[];
                    if (max > 0 && count > max) count = max;
                    des.Write(buf, _origin, count);
                    ms.Position += count;
                    return count;
                }
                // 一次读完
                bufferSize = count;
            }
            // 优化处理目标内存流，直接拿目标内存流缓冲区去源数据流里面读取数据
            if (des is MemoryStream)
            {
                var ms = des as MemoryStream;
                if (ms.TryGetValue("_origin", out var obj))
                {
                    var _origin = (int)obj;
                    // 缓冲区还剩下多少空间
                    var count = (int)(ms.Length - ms.Position);
                    // 有可能是全新的内存流
                    if (count == 0)
                    {
                        if (max > 0)
                            count = max;
                        else if (@this.CanSeek)
                        {
                            try { count = (int)(@this.Length - @this.Position); }
                            catch { count = 256; }
                        }
                        else
                            count = 256;
                        ms.Capacity += count;
                    }
                    else if (max > 0 && count > max)
                        count = max;
                    // 其实地址不为0时，一般不能直接访问缓冲区，因为可能被限制访问
                    var buf = ms.GetValue("_buffer") as byte[];
                    // 先把长度设为较大值，为后面设定长度做准备，因为直接使用SetLength会清空缓冲区
                    var len = ms.Length;
                    ms.SetLength(ms.Position + count);
                    // 直接从源数据流往这个缓冲区填充数据
                    var rs = @this.Read(buf, _origin, count);
                    if (rs > 0)
                    {
                        // 直接使用SetLength会清空缓冲区
                        ms.SetLength(ms.Position + rs);
                        ms.Position += rs;
                    }
                    else
                        ms.SetLength(len);
                    // 如果得到的数据没有达到预期，说明读完了
                    if (rs < count) return rs;
                    // 如果相等，则只有特殊情况才是达到预期
                    if (rs == count)
                    {
                        if (count != max && count != 256) return rs;
                    }
                }
            }

            if (bufferSize <= 0) bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var total = 0;
            while (true)
            {
                var count = bufferSize;
                if (max > 0)
                {
                    if (total >= max) break;

                    // 最后一次读取大小不同
                    if (count > max - total) count = max - total;
                }
                count = @this.Read(buffer, 0, count);
                if (count <= 0) break;
                total += count;
                des.Write(buffer, 0, count);
            }
            return total;
        }

        /// <summary>
        /// 把一个数据流写入到另一个数据流
        /// </summary>
        /// <param name="this">目的数据流</param>
        /// <param name="src">源数据流</param>
        /// <param name="bufferSize">缓冲区大小，也就是每次复制的大小</param>
        /// <param name="max">最大复制字节数</param>
        /// <returns></returns>
        public static Stream Write(this Stream @this, Stream src, int bufferSize = 0, int max = 0)
        {
            src.CopyTo(@this, bufferSize, max);
            return @this;
        }

        /// <summary>
        /// 把一个字节数组写入到一个数据流
        /// </summary>
        /// <param name="this">目的数据流</param>
        /// <param name="src">源数据流</param>
        /// <returns></returns>
        public static Stream Write(this Stream @this, params byte[] src)
        {
            if (src != null && src.Length > 0) @this.Write(src, 0, src.Length);
            return @this;
        }

        /// <summary>
        /// 写入字节数组，先写入压缩整数表示的长度
        /// </summary>
        /// <param name="this"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Stream WriteArray(this Stream @this, params byte[] src)
        {
            if (src == null || src.Length == 0)
            {
                @this.WriteByte(0);
                return @this;
            }
            @this.WriteEncodedInt(src.Length);
            return @this.Write(src);
        }

        /// <summary>
        /// 读取字节数组，先读取压缩整数表示的长度
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte[] ReadArray(this Stream @this)
        {
            var len = @this.ReadEncodedInt();
            if (len <= 0) return new byte[0];
            // 避免数据错乱超长
            //if (des.CanSeek && len > des.Length - des.Position) len = (int)(des.Length - des.Position);
            if (@this.CanSeek && len > @this.Length - @this.Position) throw new Exception(string.Format("ReadArray错误，变长数组长度为{0}，但数据流可用数据只有{1}", len, @this.Length - @this.Position));
            if (len > 1024 * 2) throw new Exception(string.Format("安全需要，不允许读取超大变长数组 {0:n0}>{1:n0}", len, 1024 * 2));
            return @this.ReadBytes(len);
        }

        /// <summary>
        /// 写入Unix格式时间，1970年以来秒数，绝对时间，非UTC
        /// </summary>
        /// <param name="this"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Stream WriteDateTime(this Stream @this, DateTime dt)
        {
            var seconds = dt.ToInt();
            @this.Write(seconds.GetBytes());
            return @this;
        }

        /// <summary>
        /// 读取Unix格式时间，1970年以来秒数，绝对时间，非UTC
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DateTime ReadDateTime(this Stream @this)
        {
            var buf = new byte[4];
            @this.Read(buf, 0, 4);
            var seconds = (int)buf.ToUInt32();
            return seconds.ToDateTime();
        }

        /// <summary>
        /// 复制数组
        /// </summary>
        /// <param name="this">源数组</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">复制字节数</param>
        /// <returns>返回复制的总字节数</returns>
        public static byte[] ReadBytes(this byte[] @this, int offset = 0, int count = -1)
        {
            if (count == 0) return new byte[0];
            // 即使是全部，也要复制一份，而不只是返回原数组，因为可能就是为了复制数组
            if (count < 0) count = @this.Length - offset;
            var bts = new byte[count];
            Buffer.BlockCopy(@this, offset, bts, 0, bts.Length);
            return bts;
        }

        /// <summary>
        /// 向字节数组写入一片数据
        /// </summary>
        /// <param name="this">目标数组</param>
        /// <param name="dstOffset">目标偏移</param>
        /// <param name="src">源数组</param>
        /// <param name="srcOffset">源数组偏移</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public static byte[] Write(this byte[] @this, int dstOffset, byte[] src, int srcOffset = 0, int count = -1)
        {
            if (count <= 0) count = src.Length - srcOffset;
            if (dstOffset + count > @this.Length) count = @this.Length - dstOffset;
#if MF
            Array.Copy(src, srcOffset, @this, dstOffset, count);
#else
            Buffer.BlockCopy(src, srcOffset, @this, dstOffset, count);
#endif
            return @this;
        }

        /// <summary>
        /// 合并两个数组
        /// </summary>
        /// <param name="this">源数组</param>
        /// <param name="des">目标数组</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">字节数</param>
        /// <returns></returns>
        public static byte[] Combine(this byte[] @this, byte[] des, int offset = 0, int count = -1)
        {
            if (count < 0) count = @this.Length - offset;
            var buf = new byte[@this.Length + count];
            Buffer.BlockCopy(@this, 0, buf, 0, @this.Length);
            Buffer.BlockCopy(des, offset, buf, @this.Length, count);
            return buf;
        }
        #endregion

        #region 数据流转换
        /// <summary>
        /// 数据流转为字节数组
        /// </summary>
        /// <remarks>
        /// 针对MemoryStream进行优化。内存流的Read实现是一个个字节复制，而ToArray是调用内部内存复制方法
        /// 如果要读完数据，又不支持定位，则采用内存流搬运
        /// 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
        /// </remarks>
        /// <param name="this">数据流</param>
        /// <param name="length">长度，0表示读到结束</param>
        /// <returns></returns>
        public static byte[] ReadBytes(this Stream @this, long length = -1)
        {
            if (@this == null) return null;
            if (length == 0) return new byte[0];
            if (length > 0 && @this.CanSeek && @this.Length - @this.Position < length)
                throw new Exception(string.Format("无法从长度只有{0}的数据流里面读取{1}字节的数据", @this.Length - @this.Position, length));
            if (length > 0)
            {
                var buf = new byte[length];
                var n = @this.Read(buf, 0, buf.Length);
                //if (n != buf.Length) buf = buf.ReadBytes(0, n);
                return buf;
            }
            // 如果要读完数据，又不支持定位，则采用内存流搬运
            if (!@this.CanSeek)
            {
                var ms = new MemoryStream();
                while (true)
                {
                    var buf = new byte[1024];
                    var count = @this.Read(buf, 0, buf.Length);
                    if (count <= 0) break;
                    ms.Write(buf, 0, count);
                    if (count < buf.Length) break;
                }
                return ms.ToArray();
            }
            else
            {
                // 如果指定长度超过数据流长度，就让其报错，因为那是调用者所期望的值
                length = (int)(@this.Length - @this.Position);
                var buf = new byte[length];
                @this.Read(buf, 0, buf.Length);
                return buf;
            }
        }

        /// <summary>
        /// 数据流转为字节数组，从0开始，无视数据流的当前位置
        /// </summary>
        /// <param name="this">数据流</param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream @this)
        {
            if (@this is MemoryStream)
                return (@this as MemoryStream).ToArray();
            @this.Position = 0;
            return @this.ReadBytes();
        }

        /// <summary>
        /// 从数据流中读取字节数组，直到遇到指定字节数组
        /// </summary>
        /// <param name="this">数据流</param>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">字节数组中的偏移</param>
        /// <param name="length">字节数组中的查找长度</param>
        /// <returns>未找到时返回空，0位置范围大小为0的字节数组</returns>
        public static byte[] ReadTo(this Stream @this, byte[] buffer, long offset = 0, long length = -1)
        {
            //if (!stream.CanSeek) throw new XException("流不支持查找！");
            if (length == 0) return new byte[0];
            if (length < 0) length = buffer.Length - offset;
            var ori = @this.Position;
            var p = @this.IndexOf(buffer, offset, length);
            @this.Position = ori;
            if (p < 0) return null;
            if (p == 0) return new byte[0];
            return @this.ReadBytes(p);
        }

        /// <summary>        
        /// 从数据流中读取字节数组，直到遇到指定字节数组
        /// </summary>
        /// <param name="this">数据流</param>
        /// <param name="str"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ReadTo(this Stream @this, string str, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return @this.ReadTo(encoding.GetBytes(str));
        }

        /// <summary>
        /// 从数据流中读取一行，直到遇到换行
        /// </summary>
        /// <param name="this">数据流</param>
        /// <param name="encoding"></param>
        /// <returns>未找到返回null，0位置返回string.Empty</returns>
        public static string ReadLine(this Stream @this, Encoding encoding = null)
        {
            var bts = @this.ReadTo(Environment.NewLine, encoding);
            //if (bts == null || bts.Length < 1) return null;
            if (bts == null) return null;
            @this.Seek(encoding.GetByteCount(Environment.NewLine), SeekOrigin.Current);
            if (bts.Length == 0) return string.Empty;
            return encoding.GetString(bts);
        }

        /// <summary>
        /// 流转换为字符串
        /// </summary>
        /// <param name="this">目标流</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string ToStr(this Stream @this, Encoding encoding = null)
        {
            if (@this == null) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            var buf = @this.ReadBytes();
            if (buf == null || buf.Length < 1) return null;
            // 可能数据流前面有编码字节序列，需要先去掉
            var idx = 0;
            var preamble = encoding.GetPreamble();
            if (preamble != null && preamble.Length > 0)
            {
                if (buf.StartsWith(preamble)) idx = preamble.Length;
            }
            return encoding.GetString(buf, idx, buf.Length - idx);
        }

        /// <summary>
        /// 字节数组转换为字符串
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="offset">字节数组中的偏移</param>
        /// <param name="count">字节数组中的查找长度</param>
        /// <returns></returns>
        public static string ToStr(this byte[] @this, Encoding encoding = null, int offset = 0, int count = -1)
        {
            if (@this == null || @this.Length < 1 || offset >= @this.Length) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            var size = @this.Length - offset;
            if (count < 0 || count > size) count = size;
            // 可能数据流前面有编码字节序列，需要先去掉
            var idx = 0;
            var preamble = encoding?.GetPreamble();
            if (preamble != null && preamble.Length > 0 && @this.Length >= offset + preamble.Length)
            {
                if (@this.ReadBytes(offset, preamble.Length).StartsWith(preamble)) idx = preamble.Length;
            }
            return encoding.GetString(@this, offset + idx, count - idx);
        }

        /// <summary>
        /// Stream转字符串
        /// </summary>
        /// <param name="this">Stream数据流</param>
        /// <param name="encoding">默认UTF8</param>
        /// <param name="bufferSize">默认4096</param>
        /// <returns>string</returns>
        public static string AsString(this Stream @this, Encoding encoding = null, int bufferSize = 4096)
        {
            var result = string.Empty;
            using (var reader = new StreamReader(@this, encoding ?? Encoding.UTF8, true, bufferSize, true))
            {
                if (@this.CanSeek)
                {
                    var initialPosition = @this.Position;
                    @this.Position = 0;
                    var content = reader.ReadToEnd();
                    @this.Position = initialPosition;
                    result = content;
                }
            }
            return result;
        }
        #endregion

        #region 数据转整数
        /// <summary>
        /// 从字节数据指定位置读取一个无符号16位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static ushort ToUInt16(this byte[] @this, int offset = 0, bool isLittleEndian = true)
        {
            if (isLittleEndian)
                return (ushort)((@this[offset + 1] << 8) | @this[offset]);
            else
                return (ushort)((@this[offset] << 8) | @this[offset + 1]);
        }

        /// <summary>
        /// 从字节数据指定位置读取一个无符号32位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static uint ToUInt32(this byte[] @this, int offset = 0, bool isLittleEndian = true)
        {
            if (isLittleEndian) return BitConverter.ToUInt32(@this, offset);
            // BitConverter得到小端，如果不是小端字节顺序，则倒序
            if (offset > 0) @this = @this.ReadBytes(offset, 4);
            if (isLittleEndian)
                return (uint)(@this[0] | @this[1] << 8 | @this[2] << 0x10 | @this[3] << 0x18);
            else
                return (uint)(@this[0] << 0x18 | @this[1] << 0x10 | @this[2] << 8 | @this[3]);
        }

        /// <summary>
        /// 从字节数据指定位置读取一个无符号64位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static ulong ToUInt64(this byte[] @this, int offset = 0, bool isLittleEndian = true)
        {
            if (isLittleEndian) return BitConverter.ToUInt64(@this, offset);
            if (offset > 0) @this = @this.ReadBytes(offset, 8);
            if (isLittleEndian)
            {
                var num1 = @this[0] | @this[1] << 8 | @this[2] << 0x10 | @this[3] << 0x18;
                var num2 = @this[4] | @this[5] << 8 | @this[6] << 0x10 | @this[7] << 0x18;
                return (uint)num1 | (ulong)num2 << 0x20;
            }
            else
            {
                var num3 = @this[0] << 0x18 | @this[1] << 0x10 | @this[2] << 8 | @this[3];
                var num4 = @this[4] << 0x18 | @this[5] << 0x10 | @this[6] << 8 | @this[7];
                return (uint)num4 | (ulong)num3 << 0x20;
            }
        }

        /// <summary>
        /// 向字节数组的指定位置写入一个无符号16位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="n">数字</param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static byte[] Write(this byte[] @this, ushort n, int offset = 0, bool isLittleEndian = true)
        {
            // STM32单片机是小端
            // Modbus协议规定大端
            if (isLittleEndian)
            {
                @this[offset] = (byte)(n & 0xFF);
                @this[offset + 1] = (byte)(n >> 8);
            }
            else
            {
                @this[offset] = (byte)(n >> 8);
                @this[offset + 1] = (byte)(n & 0xFF);
            }
            return @this;
        }

        /// <summary>
        /// 向字节数组的指定位置写入一个无符号32位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="n">数字</param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static byte[] Write(this byte[] @this, uint n, int offset = 0, bool isLittleEndian = true)
        {
            if (isLittleEndian)
            {
                for (var i = 0; i < 4; i++)
                {
                    @this[offset++] = (byte)n;
                    n >>= 8;
                }
            }
            else
            {
                for (var i = 4 - 1; i >= 0; i--)
                {
                    @this[offset + i] = (byte)n;
                    n >>= 8;
                }
            }
            return @this;
        }

        /// <summary>
        /// 向字节数组的指定位置写入一个无符号64位整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="n">数字</param>
        /// <param name="offset">偏移</param>
        /// <param name="isLittleEndian">是否小端字节序</param>
        /// <returns></returns>
        public static byte[] Write(this byte[] @this, ulong n, int offset = 0, bool isLittleEndian = true)
        {
            if (isLittleEndian)
            {
                for (var i = 0; i < 8; i++)
                {
                    @this[offset++] = (byte)n;
                    n >>= 8;
                }
            }
            else
            {
                for (var i = 8 - 1; i >= 0; i--)
                {
                    @this[offset + i] = (byte)n;
                    n >>= 8;
                }
            }
            return @this;
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this ushort @this, bool isLittleEndian = true)
        {
            var buf = new byte[2];
            return buf.Write(@this, 0, isLittleEndian);
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this short @this, bool isLittleEndian = true)
        {
            var buf = new byte[2];
            return buf.Write((ushort)@this, 0, isLittleEndian);
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this uint @this, bool isLittleEndian = true)
        {
            var buf = new byte[4];
            return buf.Write(@this, 0, isLittleEndian);
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this int @this, bool isLittleEndian = true)
        {
            var buf = new byte[4];
            return buf.Write((uint)@this, 0, isLittleEndian);
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this ulong @this, bool isLittleEndian = true)
        {
            var buf = new byte[8];
            return buf.Write(@this, 0, isLittleEndian);
        }

        /// <summary>
        /// 整数转为字节数组，注意大小端字节序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this long @this, bool isLittleEndian = true)
        {
            var buf = new byte[8];
            return buf.Write((ulong)@this, 0, isLittleEndian);
        }
        #endregion

        #region 7位压缩编码整数
        /// <summary>
        /// 以压缩格式读取32位整数
        /// </summary>
        /// <param name="this">数据流</param>
        /// <returns></returns>
        public static int ReadEncodedInt(this Stream @this)
        {
            byte b;
            uint rs = 0;
            byte n = 0;
            while (true)
            {
                var bt = @this.ReadByte();
                if (bt < 0) throw new Exception("数据流超出范围！已读取整数{0:n0}".F(rs));
                b = (byte)bt;

                // 必须转为int，否则可能溢出
                rs |= (uint)((b & 0x7f) << n);
                if ((b & 0x80) == 0) break;

                n += 7;
                if (n >= 32) throw new FormatException("数字值过大，无法使用压缩格式读取！");
            }
            return (int)rs;
        }

        /// <summary>
        /// 以压缩格式读取32位整数
        /// </summary>
        /// <param name="this">数据流</param>
        /// <returns></returns>
        public static ulong ReadEncodedInt64(this Stream @this)
        {
            byte b;
            ulong rs = 0;
            byte n = 0;
            while (true)
            {
                var bt = @this.ReadByte();
                if (bt < 0) throw new Exception("数据流超出范围！");
                b = (byte)bt;
                // 必须转为int，否则可能溢出
                rs |= (ulong)(b & 0x7f) << n;
                if ((b & 0x80) == 0) break;
                n += 7;
                if (n >= 64) throw new FormatException("数字值过大，无法使用压缩格式读取！");
            }
            return rs;
        }

        /// <summary>
        /// 尝试读取压缩编码整数
        /// </summary>
        /// <param name="this"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool TryReadEncodedInt(this Stream @this, out uint value)
        {
            byte b;
            value = 0;
            byte n = 0;
            while (true)
            {
                var bt = @this.ReadByte();
                if (bt < 0) return false;
                b = (byte)bt;
                // 必须转为int，否则可能溢出
                value += (uint)((b & 0x7f) << n);
                if ((b & 0x80) == 0) break;
                n += 7;
                if (n >= 32) throw new FormatException("数字值过大，无法使用压缩格式读取！");
            }
            return true;
        }

        [ThreadStatic]
        private static byte[] _encodes;
        /// <summary>
        /// 以7位压缩格式写入32位整数，小于7位用1个字节，小于14位用2个字节。
        /// 由每次写入的一个字节的第一位标记后面的字节是否还是当前数据，所以每个字节实际可利用存储空间只有后7位。
        /// </summary>
        /// <param name="this">数据流</param>
        /// <param name="value">数值</param>
        /// <returns>实际写入字节数</returns>
        public static Stream WriteEncodedInt(this Stream @this, long value)
        {
            if (_encodes == null) _encodes = new byte[16];
            var count = 0;
            var num = (ulong)value;
            while (num >= 0x80)
            {
                _encodes[count++] = (byte)(num | 0x80);
                num = num >> 7;
            }
            _encodes[count++] = (byte)num;
            @this.Write(_encodes, 0, count);
            return @this;
        }

        /// <summary>
        /// 获取压缩编码整数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetEncodedInt(long value)
        {
            if (_encodes == null) _encodes = new byte[16];
            var count = 0;
            var num = (ulong)value;
            while (num >= 0x80)
            {
                _encodes[count++] = (byte)(num | 0x80);
                num = num >> 7;
            }
            _encodes[count++] = (byte)num;
            return _encodes.ReadBytes(0, count);
        }
        #endregion

        #region 数据流查找
        /// <summary>
        /// 在数据流中查找字节数组的位置，流指针会移动到结尾
        /// </summary>
        /// <param name="this">数据流</param>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">字节数组中的偏移</param>
        /// <param name="length">字节数组中的查找长度</param>
        /// <returns></returns>
        public static long IndexOf(this Stream @this, byte[] buffer, long offset = 0, long length = -1)
        {
            if (length <= 0) length = buffer.Length - offset;
            // 位置
            long p = -1;
            for (long i = 0; i < length;)
            {
                var c = @this.ReadByte();
                if (c == -1) return -1;
                p++;
                if (c == buffer[offset + i])
                {
                    i++;

                    // 全部匹配，退出
                    if (i >= length) return p - length + 1;
                }
                else
                {
                    //i = 0; // 只要有一个不匹配，马上清零
                    // 不能直接清零，那样会导致数据丢失，需要逐位探测，窗口一个个字节滑动
                    // 上一次匹配的其实就是j=0那个，所以这里从j=1开始
                    var n = i;
                    i = 0;
                    for (var j = 1; j < n; j++)
                    {
                        // 在字节数组前(j,n)里面找自己(0,n-j)
                        if (CompareTo(buffer, j, n, buffer, 0, n - j) == 0)
                        {
                            // 前面(0,n-j)相等，窗口退回到这里
                            i = n - j;
                            break;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 在字节数组中查找另一个字节数组的位置，不存在则返回-1
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <param name="buffer">另一个字节数组</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">查找长度</param>
        /// <returns></returns>
        public static long IndexOf(this byte[] @this, byte[] buffer, long offset = 0, long length = -1) => IndexOf(@this, 0, 0, buffer, offset, length);

        /// <summary>
        /// 在字节数组中查找另一个字节数组的位置，不存在则返回-1
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <param name="start">源数组起始位置</param>
        /// <param name="count">查找长度</param>
        /// <param name="buffer">另一个字节数组</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">查找长度</param>
        /// <returns></returns>
        public static long IndexOf(this byte[] @this, long start, long count, byte[] buffer, long offset = 0, long length = -1)
        {
            if (start < 0) start = 0;
            if (count <= 0 || count > @this.Length - start) count = @this.Length;
            if (length <= 0 || length > buffer.Length - offset) length = buffer.Length - offset;
            // 已匹配字节数
            long win = 0;
            for (var i = start; i + length - win <= count; i++)
            {
                if (@this[i] == buffer[offset + win])
                {
                    win++;
                    // 全部匹配，退出
                    if (win >= length) return i - length + 1 - start;
                }
                else
                {
                    //win = 0; // 只要有一个不匹配，马上清零
                    // 不能直接清零，那样会导致数据丢失，需要逐位探测，窗口一个个字节滑动
                    i = i - win;
                    win = 0;
                }
            }
            return -1;
        }

        /// <summary>
        /// 比较两个字节数组大小。相等返回0，不等则返回不等的位置，如果位置为0，则返回1。
        /// </summary>
        /// <param name="this"></param>
        /// <param name="buffer">缓冲区</param>
        /// <returns></returns>
        public static int CompareTo(this byte[] @this, byte[] buffer) => CompareTo(@this, 0, 0, buffer, 0, 0);

        /// <summary>
        /// 比较两个字节数组大小。相等返回0，不等则返回不等的位置，如果位置为0，则返回1。
        /// </summary>
        /// <param name="this"></param>
        /// <param name="start"></param>
        /// <param name="count">数量</param>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int CompareTo(this byte[] @this, long start, long count, byte[] buffer, long offset = 0, long length = -1)
        {
            if (@this == buffer) return 0;
            if (start < 0) start = 0;
            if (count <= 0 || count > @this.Length - start) count = @this.Length - start;
            if (length <= 0 || length > buffer.Length - offset) length = buffer.Length - offset;
            // 逐字节比较
            for (var i = 0; i < count && i < length; i++)
            {
                var rs = @this[start + i].CompareTo(buffer[offset + i]);
                if (rs != 0) return i > 0 ? i : 1;
            }
            // 比较完成。如果长度不相等，则较长者较大
            if (count != length) return count > length ? 1 : -1;
            return 0;
        }

        /// <summary>
        /// 字节数组分割
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sps"></param>
        /// <returns></returns>
        public static IEnumerable<byte[]> Split(this byte[] @this, byte[] sps)
        {
            var p = 0;
            var idx = 0;
            while (true)
            {
                p = (int)@this.IndexOf(idx, 0, sps);
                if (p < 0) break;
                yield return @this.ReadBytes(idx, p);
                idx += p + sps.Length;
            }
            if (idx < @this.Length)
            {
                p = @this.Length - idx;
                yield return @this.ReadBytes(idx, p);
            }
        }

        /// <summary>
        /// 一个数据流是否以另一个数组开头。如果成功，指针移到目标之后，否则保持指针位置不变。
        /// </summary>
        /// <param name="this"></param>
        /// <param name="buffer">缓冲区</param>
        /// <returns></returns>
        public static bool StartsWith(this Stream @this, byte[] buffer)
        {
            var p = 0;
            for (var i = 0; i < buffer.Length; i++)
            {
                var b = @this.ReadByte();
                if (b == -1) { @this.Seek(-p, SeekOrigin.Current); return false; }
                p++;
                if (b != buffer[i]) { @this.Seek(-p, SeekOrigin.Current); return false; }
            }
            return true;
        }

        /// <summary>
        /// 一个数据流是否以另一个数组结尾。如果成功，指针移到目标之后，否则保持指针位置不变。
        /// </summary>
        /// <param name="this"></param>
        /// <param name="buffer">缓冲区</param>
        /// <returns></returns>
        public static bool EndsWith(this Stream @this, byte[] buffer)
        {
            if (@this.Length < buffer.Length) return false;
            var p = @this.Length - buffer.Length;
            @this.Seek(p, SeekOrigin.Current);
            if (@this.StartsWith(buffer)) return true;
            @this.Seek(-p, SeekOrigin.Current);
            return false;
        }

        /// <summary>
        /// 一个数组是否以另一个数组开头
        /// </summary>
        /// <param name="this"></param>
        /// <param name="buffer">缓冲区</param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] @this, byte[] buffer)
        {
            if (@this.Length < buffer.Length) return false;
            for (var i = 0; i < buffer.Length; i++)
            {
                if (@this[i] != buffer[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 一个数组是否以另一个数组结尾
        /// </summary>
        /// <param name="this"></param>
        /// <param name="buffer">缓冲区</param>
        /// <returns></returns>
        public static bool EndsWith(this byte[] @this, byte[] buffer)
        {
            if (@this.Length < buffer.Length) return false;
            var p = @this.Length - buffer.Length;
            for (var i = 0; i < buffer.Length; i++)
            {
                if (@this[p + i] != buffer[i]) return false;
            }
            return true;
        }
        #endregion

        #region 倒序、更换字节序
        /// <summary>
        /// 倒序、更换字节序
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <returns></returns>
        public static byte[] Reverse(this byte[] @this)
        {
            if (@this == null || @this.Length < 2) return @this;
            Array.Reverse(@this);
            return @this;
        }
        #endregion

        #region 十六进制编码
        /// <summary>
        /// 把字节数组编码为十六进制字符串
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">数量。超过实际数量时，使用实际数量</param>
        /// <returns></returns>
        public static string ToHex(this byte[] @this, int offset = 0, int count = -1)
        {
            if (@this == null || @this.Length < 1) return "";
            if (count < 0)
                count = @this.Length - offset;
            else if (offset + count > @this.Length)
                count = @this.Length - offset;
            if (count == 0) return "";
            //return BitConverter.ToString(data).Replace("-", null);
            // 上面的方法要替换-，效率太低
            var cs = new char[count * 2];
            // 两个索引一起用，避免乘除带来的性能损耗
            for (int i = 0, j = 0; i < count; i++, j += 2)
            {
                var b = @this[offset + i];
                cs[j] = GetHexValue(b / 0x10);
                cs[j + 1] = GetHexValue(b % 0x10);
            }
            return new string(cs);
        }

        /// <summary>
        /// 把字节数组编码为十六进制字符串，带有分隔符和分组功能
        /// </summary>
        /// <param name="this">字节数组</param>
        /// <param name="separate">分隔符</param>
        /// <param name="groupSize">分组大小，为0时对每个字节应用分隔符，否则对每个分组使用</param>
        /// <param name="maxLength">最大显示多少个字节。默认-1显示全部</param>
        /// <returns></returns>
        public static string ToHex(this byte[] @this, string separate, int groupSize = 0, int maxLength = -1)
        {
            if (@this == null || @this.Length < 1) return "";
            if (groupSize < 0) groupSize = 0;
            var count = @this.Length;
            if (maxLength > 0 && maxLength < count) count = maxLength;
            if (groupSize == 0 && count == @this.Length)
            {
                // 没有分隔符
                if (string.IsNullOrEmpty(separate)) return @this.ToHex();
                // 特殊处理
                if (separate == "-") return BitConverter.ToString(@this, 0, count);
            }
            var len = count * 2;
            if (!string.IsNullOrEmpty(separate)) len += (count - 1) * separate.Length;
            if (groupSize > 0)
            {
                // 计算分组个数
                var g = (count - 1) / groupSize;
                len += g * 2;
                // 扣除间隔
                if (!string.IsNullOrEmpty(separate)) len -= g * separate.Length;
            }
            var sb = new StringBuilder(len);
            for (var i = 0; i < count; i++)
            {
                if (sb.Length > 0)
                {
                    if (groupSize > 0 && i % groupSize == 0)
                        sb.AppendLine();
                    else
                        sb.Append(separate);
                }
                var b = @this[i];
                sb.Append(GetHexValue(b / 0x10));
                sb.Append(GetHexValue(b % 0x10));
            }
            return sb.ToString();
        }

        private static char GetHexValue(int i)
        {
            if (i < 10)
                return (char)(i + 0x30);
            else
                return (char)(i - 10 + 0x41);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="this">Hex编码的字符串</param>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static byte[] ToHex(this string @this, int startIndex = 0, int length = -1)
        {
            if (string.IsNullOrEmpty(@this)) return new byte[0];
            // 过滤特殊字符
            @this = @this.Trim()
                .Replace("-", null)
                .Replace("0x", null)
                .Replace("0X", null)
                .Replace(" ", null)
                .Replace("\r", null)
                .Replace("\n", null)
                .Replace(",", null);
            if (length <= 0) length = @this.Length - startIndex;
            var bts = new byte[length / 2];
            for (var i = 0; i < bts.Length; i++)
            {
                bts[i] = byte.Parse(@this.Substring(startIndex + 2 * i, 2), NumberStyles.HexNumber);
            }
            return bts;
        }
        #endregion

        #region BASE64编码
        /// <summary>
        /// 字节数组转为Base64编码
        /// </summary>
        /// <param name="this"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="lineBreak">是否换行显示</param>
        /// <returns></returns>
        public static string ToBase64(this byte[] @this, int offset = 0, int count = -1, bool lineBreak = false)
        {
            if (@this == null || @this.Length < 1) return "";
            if (count <= 0)
                count = @this.Length - offset;
            else if (offset + count > @this.Length)
                count = @this.Length - offset;
#if __CORE__
            return Convert.ToBase64String(@this, offset, count);
#else
            return Convert.ToBase64String(@this, offset, count, lineBreak ? Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None);
#endif
        }

        /// <summary>
        /// 字节数组转为Url改进型Base64编码
        /// </summary>
        /// <param name="this"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string ToUrlBase64(this byte[] @this, int offset = 0, int count = -1)
        {
            var str = ToBase64(@this, offset, count, false);
            str = str.TrimEnd('=');
            str = str.Replace('+', '-').Replace('/', '_');
            return str;
        }

        /// <summary>
        /// Base64字符串转为字节数组
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte[] ToBase64(this string @this)
        {
            if (@this.IsNullOrEmpty()) return new byte[0];
            if (@this[@this.Length - 1] != '=')
            {
                // 如果不是4的整数倍，后面补上等号
                var n = @this.Length % 4;
                if (n > 0) @this += new string('=', 4 - n);
            }
            // 针对Url特殊处理
            @this = @this.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(@this);
        }
        #endregion        
    }
}
