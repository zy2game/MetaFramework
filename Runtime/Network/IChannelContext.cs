using System.Threading.Tasks;

namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// 连接上下文
    /// </summary>
    public interface IChannelContext : GObject
    {
        IChannel Channel { get; }

        /// <summary>
        /// 将数据写入缓冲区，等待发送
        /// </summary>
        /// <param name="stream">需要写入的数据</param>
        /// <returns></returns>
        Task WriteAsync(byte[] stream);

        /// <summary>
        /// 立即将缓冲区的数据发送到远端
        /// </summary>
        void Flush();

        /// <summary>
        /// 将数据写入缓冲区,并立即将缓冲区的数据发送到远端
        /// </summary>
        /// <param name="stream">需要写入的数据</param>
        /// <returns></returns>
        Task WriteAndFlushAsync(byte[] stream);
    }
}

