#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
/****************************
* [Author] 张强
* [Date] 2020-06-17
* [Describe] Socket扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Socket扩展类
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Task<int> SendAsync(this Socket socket, byte[] buffer)
        {
            var task = Task<int>.Factory.FromAsync((byte[] buf, AsyncCallback callback, object state) =>
            {
                return socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, callback, state);
            }, socket.EndSend, buffer, null);

            return task;
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, IPEndPoint remote)
        {
            var task = Task<int>.Factory.FromAsync((byte[] buf, IPEndPoint ep, AsyncCallback callback, object state) =>
            {
                return socket.BeginSendTo(buf, 0, buf.Length, SocketFlags.None, ep, callback, state);
            }, socket.EndSendTo, buffer, remote, null);

            return task;
        }

        /// <summary>
        /// 发送数据流
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="stream"></param>
        /// <param name="remoteEP"></param>
        /// <returns>返回自身，用于链式写法</returns>
        public static Socket Send(this Socket socket, Stream stream, IPEndPoint remoteEP = null)
        {
            long total = 0;

            var size = 1472;
            var buffer = new byte[size];
            while (true)
            {
                var n = stream.Read(buffer, 0, buffer.Length);
                if (n <= 0) break;

                socket.SendTo(buffer, 0, n, SocketFlags.None, remoteEP);
                total += n;

                if (n < buffer.Length) break;
            }
            return socket;
        }

        /// <summary>
        /// 向指定目的地发送信息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer">缓冲区</param>
        /// <param name="remoteEP"></param>
        /// <returns>返回自身，用于链式写法</returns>
        public static Socket Send(this Socket socket, byte[] buffer, IPEndPoint remoteEP = null)
        {
            socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP);

            return socket;
        }

        /// <summary>
        /// 向指定目的地发送信息
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <param name="encoding">文本编码，默认null表示UTF-8编码</param>
        /// <param name="remoteEP"></param>
        /// <returns>返回自身，用于链式写法</returns>
        public static Socket Send(this Socket socket, string message, Encoding encoding = null, IPEndPoint remoteEP = null)
        {
            if (encoding == null)
                Send(socket, Encoding.UTF8.GetBytes(message), remoteEP);
            else
                Send(socket, encoding.GetBytes(message), remoteEP);
            return socket;
        }

        /// <summary>
        /// 广播数据包
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer">缓冲区</param>
        /// <param name="port"></param>
        public static Socket Broadcast(this Socket socket, byte[] buffer, int port)
        {
            if (socket != null && socket.LocalEndPoint != null)
            {
                var ip = socket.LocalEndPoint as IPEndPoint;
                if (!ip.Address.IsIPv4()) throw new NotSupportedException("IPv6不支持广播！");
            }

            if (!socket.EnableBroadcast) socket.EnableBroadcast = true;

            socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, port));

            return socket;
        }

        /// <summary>
        /// 广播字符串
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <param name="port"></param>
        public static Socket Broadcast(this Socket socket, string message, int port)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            return Broadcast(socket, buffer, port);
        }

        /// <summary>
        /// 接收字符串
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="encoding">文本编码，默认null表示UTF-8编码</param>
        /// <returns></returns>
        public static string ReceiveString(this Socket socket, Encoding encoding = null)
        {
            EndPoint ep = null;

            var buf = new byte[1460];
            var len = socket.ReceiveFrom(buf, ref ep);
            if (len < 1) return null;

            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetString(buf, 0, len);
        }

        /// <summary>
        /// 检查并开启广播
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        internal static Socket CheckBroadcast(this Socket socket, IPAddress address)
        {
            var buf = address.GetAddressBytes();
            if (buf?.Length == 4 && buf[3] == 255)
            {
                if (!socket.EnableBroadcast) socket.EnableBroadcast = true;
            }

            return socket;
        }

        #region 关闭连接
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="reuseAddress"></param>
        public static void Shutdown(this Socket socket, bool reuseAddress = false)
        {
            if (socket == null || mSafeHandle == null) return;

            var value = socket.GetValue(mSafeHandle);
            if (!(value is SafeHandle hand) || hand.IsClosed)
                return;

            // 先用Shutdown禁用Socket（发送未完成发送的数据），再用Close关闭，这是一种比较优雅的关闭Socket的方法
            if (socket.Connected)
            {
                try
                {
                    socket.Disconnect(reuseAddress);
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
            }

            socket.Close();
        }

        private static MemberInfo[] _mSafeHandle;

        /// <summary>
        /// SafeHandle字段
        /// </summary>
        private static MemberInfo mSafeHandle
        {
            get
            {
                if (_mSafeHandle != null && _mSafeHandle.Length > 0) return _mSafeHandle[0];

                MemberInfo pi = typeof(Socket).GetField("m_Handle");
                if (pi == null) pi = typeof(Socket).GetProperty("SafeHandle");
                _mSafeHandle = new MemberInfo[] { pi };

                return pi;
            }
        }
        #endregion

        #region 异步事件
        /// <summary>
        /// Socket是否未被关闭
        /// </summary>
        /// <param name="se"></param>
        /// <returns></returns>
        internal static bool IsNotClosed(this SocketAsyncEventArgs se)
        {
            return se.SocketError == SocketError.OperationAborted || se.SocketError == SocketError.Interrupted || se.SocketError == SocketError.NotSocket;
        }

        /// <summary>
        /// 根据异步事件获取可输出异常，屏蔽常见异常
        /// </summary>
        /// <param name="se"></param>
        /// <returns></returns>
        internal static Exception GetException(this SocketAsyncEventArgs se)
        {
            if (se == null) return null;

            if (se.SocketError == SocketError.ConnectionReset ||
                se.SocketError == SocketError.OperationAborted ||
                se.SocketError == SocketError.Interrupted ||
                se.SocketError == SocketError.NotSocket)
                return null;

            var ex = se.ConnectByNameError;
            if (ex == null) ex = new SocketException((int)se.SocketError);
            return ex;
        }
        #endregion

        #region 扩展方法
        /// <summary>
        /// 是否IPv4地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsIPv4(this IPAddress address) => address.AddressFamily == AddressFamily.InterNetwork;
        #endregion
    }
}