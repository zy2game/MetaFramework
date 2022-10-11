namespace GameFramework.Runtime.Network
{
    /// <summary>
    /// ���Ӵ�����
    /// </summary>
    public interface IChannelHandler : GObject
    {
        /// <summary>
        /// 网络激活
        /// </summary>
        /// <param name="context"></param>
        void ChannelActive(IChannelContext context);

        /// <summary>
        /// 网络失活
        /// </summary>
        /// <param name="context"></param>
        void ChannelInactive(IChannelContext context);

        /// <summary>
        /// 消息接收
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        void ChannelRead(IChannelContext context, byte[] message);

        /// <summary>
        /// 管道错误消息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="info"></param>
        void ChannelErrored(IChannelContext context, string info);
    }
}

