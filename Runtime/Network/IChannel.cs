using System.Net;
using System.Threading.Tasks;

namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// 网络连接管道
    /// </summary>
    /// <remarks>对WebSocket，gRPC,Tcp,UDP连接的封装</remarks>
    public interface IChannel : GObject
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        bool active { get; }

        /// <summary>
        /// 连接名
        /// </summary>
        string name { get; }

        string url { get; }

        /// <summary>
        /// 连接远端地址
        /// </summary>
        /// <returns></returns>
        Task Connect(string name, string url, IChannelHandler channelHandler);

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        Task Disconnect();

        /// <summary>
        /// 将数据写入连接管道的缓冲区，等待发送
        /// </summary>
        /// <param name="stream">需要发送的数据</param>
        /// <returns></returns>
        Task WriteAsync(byte[] stream);

        /// <summary>
        /// 立即将连接管道的数据
        /// </summary>
        void Flush();
    }
}
